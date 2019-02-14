# Export Sql DataTable to compressed CSV file Demo

How to export large SQL Server data table into a local CSV file without reading the entire table into memory and without external libraries? It is not so hard with using combination of DbCommand.ExecuteReader and StreamWriter.

For a reliable export from Azure SQL, I've also implemented **ReliableSqlDmConnection** and **ReliableSqlDbCommand** which uses [Polly](http://www.thepollyproject.org/) retry **Policy** for handling transient exceptions:

```csharp
public SqlDbRetryPolicy()
{
    _sqlRetryPolicy = Policy
        .Handle<TimeoutException>()
        .Or<SqlException>(AnyRetryableError)
        .WaitAndRetry(RETRY_COUNT, ExponentialBackoff, (exception, attempt) =>
        {
            // Capture some info for logging/telemetry.
            Console.WriteLine($"Execute: Retry {attempt} due to {exception}.");
        });
}
```
After successful export, CSV file is compressed withing origin directory using **GZipStream**.

## Prerequisites
- [Visual Studio](https://www.visualstudio.com/vs/community) 2017 15.9 or greater

## Tags & Technologies
- Sql [DbCommand.ExecuteReader](https://docs.microsoft.com/en-us/dotnet/api/system.data.sqlclient.sqlcommand.executereader?view=netframework-4.7.2)
- Transient-fault-handling with [Polly](https://github.com/App-vNext/Polly)
- Compress and decompress streams with [GZipStream](https://docs.microsoft.com/en-us/dotnet/api/system.io.compression.gzipstream?view=netframework-4.7.2)

Enjoy!

## Licence

Licenced under [MIT](http://opensource.org/licenses/mit-license.php).
Contact me on [LinkedIn](https://si.linkedin.com/in/matjazbravc)