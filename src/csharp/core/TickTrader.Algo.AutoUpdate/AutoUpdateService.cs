using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.AppCommon;
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
        private readonly string _workDir, _downloadFolder;

        private CancellationTokenSource _cancelTokenSrc;
        private List<AppUpdateEntry> _updatesCache = new();
        private DateTime _updatesCacheTimeUtc;
        private AppVersionInfo _maxVersion = AppVersionInfo.Current;
        private string _newVersion;
        private Action _newVersionCallback;
        private SynchronizationContext _newVersionCallbackContext;


        public bool HasNewVersion => _newVersion != null;

        public string NewVersion => _newVersion;


        public AutoUpdateService(string workDir)
        {
            _workDir = workDir;
            _downloadFolder = Path.Combine(workDir, "Downloads");
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

        public void SetNewVersionCallback(Action callback, bool captureContext)
        {
            _newVersionCallback = callback;
            if (captureContext)
                _newVersionCallbackContext = SynchronizationContext.Current;
        }

        public async Task<List<AppUpdateEntry>> GetUpdates(bool forced)
        {
            await CheckForUpdates(forced);
            return _updatesCache;
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


        private async Task CheckForUpdates(bool forced = false)
        {
            var cacheValid = _updatesCacheTimeUtc + UpdatesCacheLifespan > DateTime.UtcNow;
            if (!forced && cacheValid)
                return;

            await LoadUpdates();
            CheckForNewVersion();
        }

        private async Task CheckForUpdatesLoop(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                await Task.Delay(CheckUpdatesTimeout, cancelToken);

                await CheckForUpdates();
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

        private void CheckForNewVersion()
        {
            try
            {
                var notifyNewVersion = false;
                lock (_syncObj)
                {
                    // we are looking for updates higher than current version
                    var maxVersion = AppVersionInfo.Current;
                    foreach (var update in _updatesCache)
                    {
                        var updateVersion = update.Info.GetAppVersion();
                        if (maxVersion < updateVersion)
                            maxVersion = updateVersion;
                    }

                    if (maxVersion != _maxVersion)
                    {
                        // if max version changed we should rise notification
                        // updates might get deleted from providers
                        // in such cases 'null' is a reset value
                        _maxVersion = maxVersion;
                        notifyNewVersion = true;
                        _newVersion = maxVersion > AppVersionInfo.Current ? maxVersion.Version : null;
                    }
                }

                if (notifyNewVersion && _newVersionCallback != null)
                {
                    if (_newVersionCallbackContext == null)
                        _newVersionCallback();
                    else
                        _newVersionCallbackContext.Post(_ => _newVersionCallback(), null);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to check for new version");
            }
        }
    }
}
