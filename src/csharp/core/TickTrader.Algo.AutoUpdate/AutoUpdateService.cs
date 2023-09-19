using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.AppCommon.Update;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.AutoUpdate
{
    public record UpdateDownloadSource(string Name, string Uri);

    public record UpdateList(List<AppUpdateEntry> Updates, List<string> Errors);


    public class AutoUpdateService
    {
        public const string MainGithubRepo = "https://github.com/SoftFx/TTAlgo";
        public const string MainSourceName = "main";

        internal static readonly TimeSpan CheckUpdatesTimeout = TimeSpan.FromMinutes(15);
        internal static readonly TimeSpan UpdatesCacheLifespan = TimeSpan.FromMinutes(1);
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<AutoUpdateService>();

        private readonly UpdateRepository _repo;
        private readonly UpdateChecker _checker;
        private readonly UpdateInstaller _installer;

        private Action _newVersionCallback, _stateChangedCallback, _exitCallback;
        private SynchronizationContext _newVersionCallbackContext, _stateChangedCallbackContext, _exitCallbackContext;


        public bool HasNewVersion => _checker.HasNewVersion;

        public string NewVersion => _checker.NewVersion;

        public bool HasPendingUpdate => _installer.HasPendingUpdate;

        public UpdateStatusCodes? UpdateStatus => _installer.Status;

        public string UpdateStatusDetails => _installer.StatusDetails;

        public string UpdateLog => _installer.UpdateLog;


        public event Action NewVersionAvailable;
        public event Action UpdateStateChanged;


        public AutoUpdateService(string workDir, UpdateAppTypes appType)
        {
            _repo = new UpdateRepository(Path.Combine(workDir, "Downloads"));
            _checker = new UpdateChecker(_repo) { NewVersionCallback = OnNewVersionAvailable };
            _installer = new UpdateInstaller(appType, _repo, workDir)
            {
                StateChangedCallback = OnUpdateStateChanged,
                ExitCallback = OnExitCallback,
            };

            // register default source
            AddSource(new UpdateDownloadSource(MainSourceName, MainGithubRepo));
        }


        public void AddSource(UpdateDownloadSource src)
        {
            try
            {
                _repo.AddSource(src);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to add app update source '{src.Name}'");
            }
        }

        public async Task<UpdateList> GetUpdates(bool forced)
        {
            var cacheUpdated = await _repo.LoadUpdates(forced);
            if (cacheUpdated)
                _checker.CheckForNewVersion();
            return new UpdateList(_repo.UpdatesCache, _repo.LoadUpdatesErrors);
        }

        public async Task<string> DownloadUpdate(string srcId, string versionId, UpdateAssetTypes assetType)
            => await _repo.DownloadUpdate(srcId, versionId, assetType);

        public void EnableAutoCheck() => _checker.EnableAutoCheck();

        public void DisableAutoCheck() => _checker.DisableAutoCheck();

        public void SetNewVersionCallback(Action callback, bool captureContext)
        {
            _newVersionCallback = callback;
            _newVersionCallbackContext = GetCapturedContext(callback, captureContext);
        }

        public async Task InstallUpdate(string srcId, string versionId) => await _installer.InstallUpdate(srcId, versionId);

        public async Task InstallUpdateFile(string updateFilePath) => await _installer.InstallUpdateFile(updateFilePath);

        public void DiscardUpdateResult() => _installer.DiscardUpdateResult();

        public void SetUpdateStateChangedCallback(Action callback, bool captureContext)
        {
            _stateChangedCallback = callback;
            _stateChangedCallbackContext = GetCapturedContext(callback, captureContext);
        }

        public void SetExitCallback(Action callback, bool captureContext)
        {
            _exitCallback = callback;
            _exitCallbackContext = GetCapturedContext(callback, captureContext);
        }


        private SynchronizationContext GetCapturedContext(Action callback, bool captureContext)
            => (callback != null && captureContext) ? SynchronizationContext.Current : null;

        private void OnNewVersionAvailable()
        {
            try
            {
                if (_newVersionCallback == null)
                    return;

                if (_newVersionCallbackContext == null)
                    _newVersionCallback();
                else
                    _newVersionCallbackContext.Post(_ => _newVersionCallback(), null);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to execute new version callback");
            }
        }

        private void OnUpdateStateChanged()
        {
            try
            {
                if (_stateChangedCallback == null)
                    return;

                if (_stateChangedCallbackContext == null)
                    _stateChangedCallback();
                else
                    _stateChangedCallbackContext.Post(_ => _stateChangedCallback(), null);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to execute state changed callback");
            }
        }

        private void OnExitCallback()
        {
            try
            {
                if (_exitCallback == null)
                    return;

                if (_exitCallbackContext == null)
                    _exitCallback();
                else
                    _exitCallbackContext.Post(_ => _exitCallback(), null);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to execute exit callback");
            }
        }
    }
}
