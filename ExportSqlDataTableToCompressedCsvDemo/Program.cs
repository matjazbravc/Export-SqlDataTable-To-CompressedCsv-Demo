using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Autofac;
using ExportSqlDataTableToCompressedCsvDemo.Services;
using ExportSqlDataTableToCompressedCsvDemo.Services.RetryPolicies;

namespace ExportSqlDataTableToCompressedCsvDemo
{
    [ExcludeFromCodeCoverage]
    internal class Program
    {
        // Build IoC Container
        private static IContainer Container()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<SqlDbRetryPolicy>();
            builder.RegisterType<FileCompressor>().As<IFileCompressor>();
            builder.RegisterType<SqlTableDumper>().As<ISqlTableDumper>();
            return builder.Build();
        }

        private static void Main()
        {
            const string SQL_QUERY = "SELECT [FirstName], [LastName] FROM [AdventureWorks2016].[Person].[Person]";
            var dbConnectionString = ConfigurationManager.AppSettings["DbConnectionString"];
            try
            {
                var execPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (execPath == null)
                {
                    Console.WriteLine("Executing assembly location not founded.");
                    return;
                }

                // Create CSV Filename
                var dumpCsvFile = Path.Combine(execPath, "DumpCsvFile.csv");

                // Create CSV file
                var tableDumper = Container().Resolve<ISqlTableDumper>();
                tableDumper.DumpToFile(dbConnectionString, SQL_QUERY, dumpCsvFile);
                Console.WriteLine($"CSV File {dumpCsvFile} was created.");

                // Compress CSV file int the same folder
                var fileCompressor = Container().Resolve<IFileCompressor>();
                var compressedFileName = fileCompressor.Compress(dumpCsvFile);

                Console.WriteLine($"CSV File {compressedFileName} was created.");
                Console.WriteLine("\n\rPress Enter to exit.");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}