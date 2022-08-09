using Dapper;
using Hangfire;
using Hangfire.Storage;
using System.Data.Common;
using System.Reflection;

namespace HangfireHttpDemo;

public class SqlExtensionsMonitoringApi
{
    private readonly IMonitoringApi _monitoringApi;

    private static Type _type;
    private static MethodInfo _useConnection;
    private static JobStorage _jobStorage;

    public static string QUERY_TTOTAL_JOB_COUNT = $"select count(Id) from [HangFire].Job";
    public static string QUERY_JOB_BY_PAGE = $@";with cte as 
                (
                  select j.Id, row_number() over (order by j.Id desc) as row_num
                  from [HangFire].Job j
                )
                select j.*, s.Reason as StateReason, s.Data as StateData, s.CreatedAt as StateChanged
                from [HangFire].Job j
                inner join cte on cte.Id = j.Id
                left join [HangFire].State s on j.StateId = s.Id and j.Id = s.JobId
                where cte.row_num between @start and @end";

    public SqlExtensionsMonitoringApi()
    {
        _jobStorage = JobStorage.Current;
        if (_jobStorage == null)
        {
            throw new ArgumentException("The function jobStorage cannot be found.");
        }

        _monitoringApi = _jobStorage.GetMonitoringApi();
        if (_monitoringApi.GetType().Name != "SqlServerMonitoringApi")
        {
            throw new ArgumentException("The monitor API is not implemented using SQL Server", nameof(IMonitoringApi));
        }

        if (_type != _monitoringApi.GetType())
        {
            _useConnection = null;
            _type = _monitoringApi.GetType();
        }

        if (_useConnection == null)
        {
            _useConnection = _monitoringApi.GetType().GetTypeInfo().GetMethod(nameof(UseConnection), BindingFlags.NonPublic | BindingFlags.Instance);
        }

        if (_useConnection == null)
        {
            throw new ArgumentException("The function UseConnection cannot be found.");
        }
    }

    private T UseConnection<T>(Func<DbConnection, T> action)
    {
        var method = _useConnection.MakeGenericMethod(typeof(T));
        return (T)method.Invoke(_monitoringApi, new object[] { action });
    }

    public int GetAllJobsCount()
    {
        return UseConnection(connection =>
        {
            var count = connection.ExecuteScalar<int>(QUERY_TTOTAL_JOB_COUNT);
            return count;
        });
    }

    public List<SqlJob> GetAllJobsByPage(int from, int count)
    {
        return UseConnection(connection =>
        {
            return connection.Query<SqlJob>(QUERY_JOB_BY_PAGE, new { start = @from + 1, end = @from + count }).ToList();
        });
    }
}
