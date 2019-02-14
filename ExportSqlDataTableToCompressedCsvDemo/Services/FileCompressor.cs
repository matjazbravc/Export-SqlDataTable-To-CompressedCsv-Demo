using System.IO;
using System.IO.Compression;

namespace ExportSqlDataTableToCompressedCsvDemo.Services
{
    /// <summary>
    /// File Compressor/Decompresor using GZipStream.
    /// Does not require any external libraries.
    /// </summary>
    public class FileCompressor : IFileCompressor
    {
        public string Compress(string fileNameToCompress)
        {
            var file = new FileInfo(fileNameToCompress);
            using (var fileStream = file.OpenRead())
            {
                if (!(((File.GetAttributes(file.FullName) & FileAttributes.Hidden) != FileAttributes.Hidden) & (file.Extension != ".gz")))
                {
                    return string.Empty;
                }
                var compressedFileName = file.FullName + ".gz";
                using (var compressedFileStream = File.Create(file.FullName + ".gz"))
                {
                    using (var compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                    {
                        fileStream.CopyTo(compressionStream);
                        return compressedFileName;
                    }
                }
            }
        }

        public string Decompress(string fileNameToDecompress)
        {
            var file = new FileInfo(fileNameToDecompress);
            using (var fileStream = file.OpenRead())
            {
                var currentFileName = file.FullName;
                var newFileName = currentFileName.Remove(currentFileName.Length - file.Extension.Length);
                using (var decompressedFileStream = File.Create(newFileName))
                {
                    using (var decompressionStream = new GZipStream(fileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                        return file.Name;
                    }
                }
            }
        }
    }
}