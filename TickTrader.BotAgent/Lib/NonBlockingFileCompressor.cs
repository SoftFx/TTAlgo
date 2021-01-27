using NLog;
using NLog.Targets;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TickTrader.BotAgent
{
    public class NonBlockingFileCompressor : IFileCompressor
    {
        private static readonly ILogger _logger = LogManager.GetLogger(nameof(NonBlockingFileCompressor));

        private Channel<CompressRequest> _requestChannel;


        public NonBlockingFileCompressor()
        {
            var options = new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = false,
                SingleReader = true,
                SingleWriter = false,
            };
            _requestChannel = Channel.CreateUnbounded<CompressRequest>(options);

            var _ = ListenCompressRequests();
        }


        public static void Setup()
        {
            FileTarget.FileCompressor = new NonBlockingFileCompressor();
        }


        public void CompressFile(string fileName, string archiveFileName)
        {
            var rawFile = $"{archiveFileName}.log";
            File.Move(fileName, rawFile);

            var request = new CompressRequest(rawFile, archiveFileName, Path.GetFileName(fileName));
            const int maxAttempts = 10;
            var cnt = 0;
            while (cnt < maxAttempts && !_requestChannel.Writer.TryWrite(request)) cnt++;
            if (cnt == maxAttempts)
            {
                _logger.Error($"Failed to queue file '{rawFile}' to archivation channel");
            }
            else
            {
                _logger.Info($"Added file '{rawFile}' to archivation channel");
            }
        }


        private async Task ListenCompressRequests()
        {
            var reader = _requestChannel.Reader;
            while(await reader.WaitToReadAsync())
            {
                try
                {
                    await Task.Factory.StartNew(CompressFiles, TaskCreationOptions.LongRunning);
                }
                catch(Exception ex)
                {
                    _logger.Error(ex, "Failed to compress files");
                }
            }
        }


        public void CompressFiles()
        {
            var reader = _requestChannel.Reader;
            while (reader.TryRead(out var request))
            {
                var rawFile = request.File;
                try
                {
                    var sw = Stopwatch.StartNew();

                    using (var archiveStream = new FileStream(request.ArchieveFile, FileMode.Create))
                    using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create))
                    using (var originalFileStream = new FileStream(rawFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var zipArchiveEntry = archive.CreateEntry(request.FileName);
                        using (var destination = zipArchiveEntry.Open())
                        {
                            originalFileStream.CopyTo(destination);
                        }
                    }
                    File.Delete(rawFile);

                    sw.Stop();
                    _logger.Info($"Compression took {sw.ElapsedTicks * 1e-4} ms. File: {rawFile}");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Failed to compress file {rawFile}");
                }
            }
        }


        private struct CompressRequest
        {
            public string File { get; }

            public string ArchieveFile { get; }

            public string FileName { get; }


            public CompressRequest(string file, string archieveFile, string fileName) : this()
            {
                File = file;
                ArchieveFile = archieveFile;
                FileName = fileName;
            }
        }
    }
}
