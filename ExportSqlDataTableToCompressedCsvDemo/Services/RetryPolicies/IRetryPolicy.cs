using System;

namespace ExportSqlDataTableToCompressedCsvDemo.Services.RetryPolicies
{
    // inspired by: http://sergeyakopov.com/reliable-database-connections-and-commands-with-polly/
    public interface IRetryPolicy
    {
        void Execute(Action action);

        TResult Execute<TResult>(Func<TResult> action);
    }
}
