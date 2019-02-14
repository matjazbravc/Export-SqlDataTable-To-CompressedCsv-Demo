using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using ExportSqlDataTableToCompressedCsvDemo.Services.RetryPolicies;

namespace ExportSqlDataTableToCompressedCsvDemo.Services.ReliableSql
{
    // inspired by: http://sergeyakopov.com/reliable-database-connections-and-commands-with-polly/
    public class ReliableSqlDbCommand : DbCommand
    {
        private readonly IRetryPolicy _retryPolicy;
        private readonly SqlCommand _underlyingSqlCommand;

        public ReliableSqlDbCommand(SqlCommand command, IRetryPolicy retryPolicy)
        {
            _underlyingSqlCommand = command;
            _retryPolicy = retryPolicy;
        }

        public override string CommandText
        {
            get => _underlyingSqlCommand.CommandText;
            set => _underlyingSqlCommand.CommandText = value;
        }

        public override int CommandTimeout
        {
            get => _underlyingSqlCommand.CommandTimeout;
            set => _underlyingSqlCommand.CommandTimeout = value;
        }

        public override CommandType CommandType
        {
            get => _underlyingSqlCommand.CommandType;
            set => _underlyingSqlCommand.CommandType = value;
        }

        public override bool DesignTimeVisible
        {
            get => _underlyingSqlCommand.DesignTimeVisible;
            set => _underlyingSqlCommand.DesignTimeVisible = value;
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get => _underlyingSqlCommand.UpdatedRowSource;
            set => _underlyingSqlCommand.UpdatedRowSource = value;
        }

        protected override DbConnection DbConnection
        {
            get => _underlyingSqlCommand.Connection;
            set => _underlyingSqlCommand.Connection = (SqlConnection)value;
        }

        protected override DbParameterCollection DbParameterCollection => _underlyingSqlCommand.Parameters;

        protected override DbTransaction DbTransaction
        {
            get => _underlyingSqlCommand.Transaction;
            set => _underlyingSqlCommand.Transaction = (SqlTransaction)value;
        }

        public override void Cancel()
        {
            _underlyingSqlCommand.Cancel();
        }

        public override int ExecuteNonQuery()
        {
            return _retryPolicy.Execute(() => _underlyingSqlCommand.ExecuteNonQuery());
        }

        public override object ExecuteScalar()
        {
            return _retryPolicy.Execute(() => _underlyingSqlCommand.ExecuteScalar());
        }

        public override void Prepare()
        {
            _retryPolicy.Execute(() => _underlyingSqlCommand.Prepare());
        }

        protected override DbParameter CreateDbParameter()
        {
            return _underlyingSqlCommand.CreateParameter();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _underlyingSqlCommand.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return _retryPolicy.Execute(() => _underlyingSqlCommand.ExecuteReader(behavior));
        }
    }
}