using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using ExportSqlDataTableToCompressedCsvDemo.Services.ReliableSql;
using ExportSqlDataTableToCompressedCsvDemo.Services.RetryPolicies;

namespace ExportSqlDataTableToCompressedCsvDemo.Services
{
    /// <summary>
    /// Table to CSV File exporter that will scale very well to large files, because it avoids reading the entire table into memory.
    /// Does not require any external libraries.
    /// </summary>
    public class SqlTableDumper : ISqlTableDumper
    {
        private readonly SqlDbRetryPolicy _sqlDbRetryPolicy;

        public SqlTableDumper(SqlDbRetryPolicy sqlDbRetryPolicy)
        {
            _sqlDbRetryPolicy = sqlDbRetryPolicy;
        }

        public void DumpToFile(string connectionString, string sqlQuery, string destinationFile, string separator = ";")
        {
            using (var connection = new ReliableSqlDbConnection(connectionString, _sqlDbRetryPolicy))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sqlQuery;
                    command.CommandTimeout = 600; // 10 minutes
                    command.CommandType = CommandType.Text;
                    using (var reader = command.ExecuteReader())
                    {
                        using (var outFile = File.CreateText(destinationFile))
                        {
                            var columnNames = GetTableColumnNames(reader).ToArray();
                            var numFields = columnNames.Length;
                            outFile.WriteLine(string.Join(separator, columnNames));
                            if (!reader.HasRows)
                            {
                                return;
                            }
                            while (reader.Read())
                            {
                                var columnValues =
                                    Enumerable.Range(0, numFields)
                                        .Select(i => reader.GetValue(i).ToString())
                                        .Select(field => string.Concat("\"", field.Replace("\"", "\"\""), "\""))
                                        .ToArray();
                                outFile.WriteLine(string.Join(separator, columnValues));
                            }
                        }
                    }
                }
            }
        }

        private static IEnumerable<string> GetTableColumnNames(IDataReader reader)
        {
            var dataRowCollection = reader.GetSchemaTable()?.Rows;
            if (dataRowCollection == null)
            {
                yield break;
            }
            foreach (DataRow row in dataRowCollection)
            {
                yield return (string)row["ColumnName"];
            }
        }
    }
}