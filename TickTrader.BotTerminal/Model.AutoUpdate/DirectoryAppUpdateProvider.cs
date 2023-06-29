using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Threading.Tasks;
using TickTrader.Algo.AppCommon.Update;
using TickTrader.Algo.Core.Lib;
using System.Text.Json;

namespace TickTrader.BotTerminal.Model.AutoUpdate
{
    internal class DirectoryAppUpdateProvider : IAppUpdateProvider
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<DirectoryAppUpdateProvider>();

        private readonly string _srcId, _path;


        public DirectoryAppUpdateProvider(UpdateDownloadSource updateSrc)
        {
            _srcId = updateSrc.Name;
            _path = updateSrc.Uri;
        }


        public Task<List<AppUpdateEntry>> GetUpdates() => Task.Run(() => GetUpdatesInternal(_srcId, _path));

        public Task Download(string subLink, string dstPath)
        {
            var srcPath = Path.Combine(_path, subLink);
            File.Copy(srcPath, dstPath, true);
            return Task.CompletedTask;
        }


        private static List<AppUpdateEntry> GetUpdatesInternal(string srcId, string path)
        {
            var res = new List<AppUpdateEntry>();
            var updateCandidates = Directory.GetFiles(path, "Algo*.Update.zip");
            try
            {
                foreach (var updPath in updateCandidates)
                {
                    var filename = Path.GetFileName(updPath);
                    UpdateAppTypes? appType = null;
                    if (filename.StartsWith("AlgoTerminal"))
                        appType = UpdateAppTypes.Terminal;
                    if (filename.StartsWith("AlgoServer"))
                        appType = UpdateAppTypes.Server;

                    if (appType.HasValue)
                    {
                        if (TryLoadUpdateInfoFromZip(updPath, out var updInfo))
                        {
                            var fileName = Path.GetFileName(updPath);
                            res.Add(new AppUpdateEntry
                            {
                                SrcId = srcId,
                                SubLink = updPath,
                                Info = updInfo,
                                AppType = appType.Value,
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get updates from local folder");
            }
            return res;
        }

        private static bool TryLoadUpdateInfoFromZip(string zipPath, out UpdateInfo updateInfo)
        {
            updateInfo = default;
            try
            {
                using var zipFile = File.Open(zipPath, FileMode.Open, FileAccess.Read);
                using var zip = new ZipArchive(zipFile);
                var entry = zip.GetEntry(UpdateHelper.InfoFileName);
                if (entry == null)
                    return false;
                using var entryStream = entry.Open();
                updateInfo = JsonSerializer.Deserialize<UpdateInfo>(entryStream);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to read update info from '{zipPath}'");
            }
            return false;
        }
    }
}
