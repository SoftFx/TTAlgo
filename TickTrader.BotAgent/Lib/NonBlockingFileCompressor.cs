using NLog;
using NLog.Targets;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace TickTrader.BotAgent
{
    public class NonBlockingFileCompressor : IFileCompressor
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();


        public static void Setup()
        {
            FileTarget.FileCompressor = new NonBlockingFileCompressor();
        }


        public void CompressFile(string fileName, string archiveFileName)
        {
            var rawFile = $"{archiveFileName}.log";
            File.Move(fileName, rawFile);
            Task.Run(() =>
            {
                try
                {
                    using (var archiveStream = new FileStream(archiveFileName, FileMode.Create))
                    using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create))
                    using (var originalFileStream = new FileStream(rawFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var zipArchiveEntry = archive.CreateEntry(Path.GetFileName(fileName));
                        using (var destination = zipArchiveEntry.Open())
                        {
                            originalFileStream.CopyTo(destination);
                        }
                    }
                    File.Delete(rawFile);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Failed to archive file {rawFile}");
                }
            });
        }
    }
}
