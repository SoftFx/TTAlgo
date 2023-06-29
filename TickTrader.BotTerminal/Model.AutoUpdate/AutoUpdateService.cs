using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotTerminal.Model.AutoUpdate
{
    internal class UpdateDownloadSource
    {
        public string Name { get; set; }

        public string Uri { get; set; }
    }


    internal class AutoUpdateService
    {
        private static readonly TimeSpan CheckUpdatesTimeout = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan UpdatesCacheLifespan = TimeSpan.FromMinutes(1);
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<AutoUpdateService>();

        private readonly object _syncObj = new();
        private readonly Dictionary<string, IAppUpdateProvider> _providers = new();

        private CancellationTokenSource _cancelTokenSrc;
        private List<AppUpdateEntry> _updatesCache = new();
        private DateTime _updatesCacheTimeUtc;


        public AutoUpdateService()
        {
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

        public async Task<string> DownloadUpdate(AppUpdateEntry entry)
        {
            IAppUpdateProvider provider = default;
            lock(_syncObj)
            {
                _ = _providers.TryGetValue(entry.SrcId, out provider);
            }
            if (provider == null)
                throw new ArgumentException("Invalid source id");

            var cachePath = Path.Combine(EnvService.Instance.UpdatesCacheFolder, $"{entry.AppType} {entry.Info.ReleaseVersion}.zip");
            if (File.Exists(cachePath))
            {
                // TODO: validate checksum
            }
            else
            {
                // TODO: cleanup cache
                await provider.Download(entry.SubLink, cachePath);
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
                var tasks = new List<Task<List<AppUpdateEntry>>>(_providers.Count * 2);
                lock (_syncObj)
                {
                    foreach (var provider in _providers.Values)
                        tasks.Add(provider.GetUpdates());
                }
                await Task.WhenAll(tasks);
                var updates = tasks.Where(t => t.IsCompletedSuccessfully).SelectMany(t => t.Result).ToList();
                lock (_syncObj)
                {
                    _updatesCache = updates;
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
