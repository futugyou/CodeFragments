
using Microsoft.Agents.AI.Hosting;
using Npgsql;

namespace AgentStack.SessionStore;

public sealed class PostgresAgentSessionStore : AgentSessionStore
{
    private readonly NpgsqlDataSource _dataSource;
    private const string TableName = "agent_sessions";

    public PostgresAgentSessionStore(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        const string sql = $@"
            CREATE TABLE IF NOT EXISTS {TableName} (
                key TEXT PRIMARY KEY,
                session_data JSONB NOT NULL,
                updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
            );
            CREATE INDEX IF NOT EXISTS idx_agent_sessions_updated_at ON {TableName} (updated_at);";

        await using var command = _dataSource.CreateCommand(sql);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public override async ValueTask SaveSessionAsync(AIAgent agent, string conversationId, AgentSession session, CancellationToken cancellationToken = default)
    {
        var key = GetKey(conversationId, agent.Id);
        Console.WriteLine($"Saving session: {key}");
        var jsonString = JsonSerializer.Serialize(session.Serialize());

        const string sql = $@"
            INSERT INTO {TableName} (key, session_data)
            VALUES (@key, @data::jsonb)
            ON CONFLICT (key) DO UPDATE SET 
                session_data = EXCLUDED.session_data,
                updated_at = CURRENT_TIMESTAMP;";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("key", key);
        command.Parameters.AddWithValue("data", jsonString);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public override async ValueTask<AgentSession> GetSessionAsync(AIAgent agent, string conversationId, CancellationToken cancellationToken = default)
    {
        var key = GetKey(conversationId, agent.Id);
        Console.WriteLine($"Getting session: {key}");

        const string sql = $"SELECT session_data FROM {TableName} WHERE key = @key";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("key", key);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            var jsonString = reader.GetString(0);
            using var doc = JsonDocument.Parse(jsonString);
            return await agent.DeserializeSessionAsync(doc.RootElement.Clone());
        }

        return await agent.GetNewSessionAsync();
    }

    private static string GetKey(string conversationId, string agentId) => $"{agentId}:{conversationId}";
}