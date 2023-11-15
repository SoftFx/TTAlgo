using System;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.AppCommon;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.AutoUpdate
{
    internal class UpdateChecker
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<UpdateChecker>();

        private readonly object _syncObj = new();
        private readonly UpdateRepository _repo;

        private CancellationTokenSource _cancelTokenSrc;
        private AppVersionInfo _maxVersion = AppVersionInfo.Current;
        private string _newVersion;


        public Action NewVersionCallback { get; set; }

        public bool HasNewVersion => _newVersion != null;

        public string NewVersion => _newVersion;


        public UpdateChecker(UpdateRepository repo)
        {
            _repo = repo;
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

        public void CheckForNewVersion()
        {
            try
            {
                var updates = _repo.UpdatesCache;
                var notifyNewVersion = false;
                lock (_syncObj)
                {
                    // we are looking for updates higher than current version
                    var maxVersion = AppVersionInfo.Current;
                    foreach (var update in updates)
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

                if (notifyNewVersion && NewVersionCallback != null)
                    NewVersionCallback();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to check for new version");
            }
        }


        private async Task CheckForUpdatesLoop(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                await Task.Delay(AutoUpdateService.CheckUpdatesTimeout, cancelToken);

                await _repo.LoadUpdates(false);
                CheckForNewVersion();
            }
        }
    }
}
