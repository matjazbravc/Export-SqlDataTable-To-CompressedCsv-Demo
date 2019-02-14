namespace ExportSqlDataTableToCompressedCsvDemo.Services
{
    public interface ISqlTableDumper
    {
        void DumpToFile(string connectionString, string sqlQuery, string destinationFile, string separator = ";");
    }
}
