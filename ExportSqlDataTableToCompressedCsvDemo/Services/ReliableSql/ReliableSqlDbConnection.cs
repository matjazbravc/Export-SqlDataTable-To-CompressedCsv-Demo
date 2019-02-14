using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using ExportSqlDataTableToCompressedCsvDemo.Services.RetryPolicies;

namespace ExportSqlDataTableToCompressedCsvDemo.Services.ReliableSql
{
    // inspired by: http://sergeyakopov.com/reliable-database-connections-and-commands-with-polly/
    public class ReliableSqlDbConnection : DbConnection
    {
        private readonly IRetryPolicy _retryPolicy;
        private readonly SqlConnection _underlyingConnection;
        private string _connectionString;

        public ReliableSqlDbConnection(string connectionString, IRetryPolicy retryPolicy)
        {
            _retryPolicy = retryPolicy;
            _connectionString = BuildConnectionString(connectionString);
            _underlyingConnection = new SqlConnection(_connectionString);
        }

        public override string ConnectionString
        {
            get => _connectionString;
            set
            {
                _connectionString = BuildConnectionString(value);
                _underlyingConnection.ConnectionString = _connectionString;
            }
        }

        public override string Database => _underlyingConnection.Database;

        public override string DataSource => _underlyingConnection.DataSource;

        public override string ServerVersion => _underlyingConnection.ServerVersion;

        public override ConnectionState State => _underlyingConnection.State;

        public override void ChangeDatabase(string databaseName)
        {
            _underlyingConnection.ChangeDatabase(databaseName);
        }

        public override void Close()
        {
            _underlyingConnection.Close();
        }

        public override void Open()
        {
            _retryPolicy.Execute(() =>
            {
                if (_underlyingConnection.State != ConnectionState.Open)
                {
                    _underlyingConnection.Open();
                }
            });
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return _underlyingConnection.BeginTransaction(isolationLevel);
        }

        protected override DbCommand CreateDbCommand()
        {
            return new ReliableSqlDbCommand(_underlyingConnection.CreateCommand(), _retryPolicy);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_underlyingConnection.State == ConnectionState.Open)
                {
                    _underlyingConnection.Close();
                }

                _underlyingConnection.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        private static string BuildConnectionString(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                // Gets or sets the length of time (in seconds) to wait for a connection to the server before terminating the attempt and generating an error.
                ConnectTimeout = 300, // 5 mins
                Pooling = true
            };
            return builder.ConnectionString;
        }
    }
}