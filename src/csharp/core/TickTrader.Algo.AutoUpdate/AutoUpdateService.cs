using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.AutoUpdate
{
    public class UpdateDownloadSource
    {
        public string Name { get; set; }

        public string Uri { get; set; }
    }


    public class AutoUpdateService
    {
        public const string MainGithubRepo = "https://github.com/SoftFx/TTAlgo";
        public const string MainSourceName = "main";

        private static readonly TimeSpan CheckUpdatesTimeout = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan UpdatesCacheLifespan = TimeSpan.FromMinutes(1);
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<AutoUpdateService>();

        private readonly object _syncObj = new();
        private readonly Dictionary<string, IAppUpdateProvider> _providers = new();
        private readonly string _cacheFolder;

        private CancellationTokenSource _cancelTokenSrc;
        private List<AppUpdateEntry> _updatesCache = new();
        private DateTime _updatesCacheTimeUtc;


        public AutoUpdateService(string cacheFolder)
        {
            _cacheFolder = cacheFolder;
        }


        public void AddSource(UpdateDownloadSource src)
        {
            lock (_syncObj)
            {
                if (_providers.ContainsKey(src.Name))
                    throw new ArgumentException("Duplicate source name");

                _providers.Add(src.Name, AppUpdateProvider.Create(src));
                _updatesCacheTimeUtc = DateTime.MinValue; // reset cache validity
            }
        }

        public async Task<List<AppUpdateEntry>> GetUpdates(bool forced = false)
        {
            var cacheValid = _updatesCacheTimeUtc + UpdatesCacheLifespan > DateTime.UtcNow;
            if (forced || !cacheValid)
            {
                await LoadUpdates();
            }
            return _updatesCache;
        }

        public async Task<string> DownloadUpdate(AppUpdateEntry entry, UpdateAssetTypes assetType)
        {
            return await DownloadUpdate(entry.SrcId, entry.VersionId, assetType);
        }

        public async Task<string> DownloadUpdate(string srcId, string versionId, UpdateAssetTypes assetType)
        {
            IAppUpdateProvider provider = default;
            lock(_syncObj)
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
                UpdateAssetTypes.Setup => $"Algo Studio {entry.Info.ReleaseVersion}.exe",
                _ => throw new NotSupportedException()
            };
            var cachePath = Path.Combine(_cacheFolder, filename);
            var loadFile = true;
            if (File.Exists(cachePath))
            {
                // TODO: validate checksum
                loadFile = false;
            }
            if (loadFile)
            {
                // TODO: cleanup cache
                await provider.Download(versionId, assetType, cachePath);
            }

            return cachePath;
        }

        public void EnableAutoCheck()
        {
            lock (_syncObj)
            {
                if (_cancelTokenSrc != null)
                    return;

                _cancelTokenSrc = new CancellationTokenSource();
                _ = Task.Run(() => CheckForUpdatesLoop(_cancelTokenSrc.Token));
            }
        }

        public void DisableAutoCheck()
        {
            lock (_syncObj)
            {
                if (_cancelTokenSrc == null)
                    return;

                _cancelTokenSrc.Cancel();
                _cancelTokenSrc = null;
            }
        }


        private async Task LoadUpdates()
        {
            try
            {
                var tasks = new List<Task>(_providers.Count * 2);
                lock (_syncObj)
                {
                    foreach (var provider in _providers.Values)
                        tasks.Add(provider.LoadUpdates());
                }
                await Task.WhenAll(tasks);
                lock (_syncObj)
                {
                    _updatesCache = _providers.Values.SelectMany(p => p.Updates).ToList();
                    _updatesCacheTimeUtc = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load updates");
            }
        }

        private async Task CheckForUpdatesLoop(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                await Task.Delay(CheckUpdatesTimeout, cancelToken);

                await CheckForUpdates();
            }
        }

        private async Task CheckForUpdates()
        {
            try
            {
                await LoadUpdates();

                // TODO: Add notification for new version
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to check for updates");
            }
        }
    }
}
