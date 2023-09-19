using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.AutoUpdate
{
    internal class UpdateRepository
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<UpdateRepository>();

        private readonly object _syncObj = new();
        private readonly Dictionary<string, IAppUpdateProvider> _providers = new();
        private readonly string _downloadFolder;

        private DateTime _updatesCacheTimeUtc;


        public List<AppUpdateEntry> UpdatesCache { get; private set; } = new();

        public List<string> LoadUpdatesErrors { get; private set; } = new();


        public UpdateRepository(string downloadFolder)
        {
            _downloadFolder = downloadFolder;
        }


        public void AddSource(UpdateDownloadSource src)
        {
            lock (_syncObj)
            {
                if (_providers.ContainsKey(src.Name))
                {
                    _logger.Error($"App update provider with name '{src.Name}' already exists. Skip adding.");
                    return;
                }

                _providers.Add(src.Name, AppUpdateProvider.Create(src));
                _updatesCacheTimeUtc = DateTime.MinValue; // reset cache validity
            }
        }

        public async Task<bool> LoadUpdates(bool forced)
        {
            try
            {
                var cacheValid = _updatesCacheTimeUtc + AutoUpdateService.UpdatesCacheLifespan > DateTime.UtcNow;
                if (!forced && cacheValid)
                    return false;

                var tasks = new List<Task>(_providers.Count * 2);
                lock (_syncObj)
                {
                    foreach (var provider in _providers.Values)
                        tasks.Add(provider.LoadUpdates());
                }
                await Task.WhenAll(tasks);
                lock (_syncObj)
                {
                    var newUpdateList = _providers.Values.SelectMany(p => p.Updates).ToList();
                    var newErrorsList = _providers.Values.Select(p => p.LoadUpdatesError).Where(e => !string.IsNullOrEmpty(e)).ToList();
                    UpdatesCache = newUpdateList;
                    LoadUpdatesErrors = newErrorsList;
                    _updatesCacheTimeUtc = DateTime.UtcNow;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load updates");
            }

            return false;
        }

        public async Task<string> DownloadUpdate(string srcId, string versionId, UpdateAssetTypes assetType)
        {
            IAppUpdateProvider provider = default;
            lock (_syncObj)
            {
                _ = _providers.TryGetValue(srcId, out provider);
            }
            if (provider == null)
                throw new ArgumentException("Invalid source id");
            var entry = provider.GetUpdate(versionId);
            if (entry == null)
                throw new ArgumentException("Invalid version id");
            if (!entry.AvailableAssets.Contains(assetType))
                throw new ArgumentException("Invalid asset type");

            var filename = assetType switch
            {
                UpdateAssetTypes.TerminalUpdate => $"AlgoTerminal {entry.Info.ReleaseVersion}.Update.zip",
                UpdateAssetTypes.ServerUpdate => $"AlgoServer {entry.Info.ReleaseVersion}.Update.zip",
                UpdateAssetTypes.Setup => $"Algo Studio {entry.Info.ReleaseVersion}.Setup.exe",
                _ => throw new NotSupportedException()
            };
            var downloadPath = Path.Combine(_downloadFolder, filename);
            PathHelper.EnsureDirectoryCreated(_downloadFolder);
            if (File.Exists(downloadPath))
                File.Delete(downloadPath);

            await provider.Download(versionId, assetType, downloadPath);

            return downloadPath;
        }
    }
}
