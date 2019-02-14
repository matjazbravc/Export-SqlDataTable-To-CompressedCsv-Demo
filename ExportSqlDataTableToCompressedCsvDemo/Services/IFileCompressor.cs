
namespace ExportSqlDataTableToCompressedCsvDemo.Services
{
    public interface IFileCompressor
    {
        string Compress(string fileNameToCompress);

        string Decompress(string fileNameToDecompress);
    }
}
