
using Microsoft.Agents.AI.Hosting;
using Npgsql;

namespace ThreadStore;

public sealed class PostgresAgentThreadStore : AgentThreadStore
{
    private readonly NpgsqlDataSource _dataSource;
    private const string TableName = "agent_threads";

    public PostgresAgentThreadStore(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public override async ValueTask SaveThreadAsync(AIAgent agent, string conversationId, AgentThread thread, CancellationToken cancellationToken = default)
    {
        var key = GetKey(conversationId, agent.Id);
        var jsonString = JsonSerializer.Serialize(thread.Serialize());

        const string sql = $@"
            INSERT INTO {TableName} (key, thread_data)
            VALUES (@key, @data::jsonb)
            ON CONFLICT (key) DO UPDATE SET 
                thread_data = EXCLUDED.thread_data,
                updated_at = CURRENT_TIMESTAMP;";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("key", key);
        command.Parameters.AddWithValue("data", jsonString);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public override async ValueTask<AgentThread> GetThreadAsync(AIAgent agent, string conversationId, CancellationToken cancellationToken = default)
    {
        var key = GetKey(conversationId, agent.Id);

        const string sql = $"SELECT thread_data FROM {TableName} WHERE key = @key";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("key", key);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            var jsonString = reader.GetString(0);
            using var doc = JsonDocument.Parse(jsonString);
            return agent.DeserializeThread(doc.RootElement.Clone());
        }

        return agent.GetNewThread();
    }

    private static string GetKey(string conversationId, string agentId) => $"{agentId}:{conversationId}";
}