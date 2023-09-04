using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.AppCommon;
using TickTrader.Algo.AppCommon.Update;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.AutoUpdate
{
    internal class UpdateInstaller
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<UpdateInstaller>();
        private static readonly TimeSpan PendingUpdateTimeout = TimeSpan.FromMinutes(10);

        private readonly object _syncObj = new();
        private readonly string _workDir;
        private readonly UpdateAppTypes _appType;
        private readonly UpdateRepository _repo;
        private readonly UpdateLogIO _updateLogIO;

        private UpdateState _state;
        private CancellationTokenSource _watchUpdateCancelTokenSrc;


        public bool HasPendingUpdate { get; private set; }

        public UpdateStatusCodes? Status => _state?.Status;

        public string StatusDetails { get; private set; }

        public string UpdateLog { get; private set; }

        public Action StateChangedCallback { get; set; }


        public UpdateInstaller(UpdateAppTypes appType, UpdateRepository repo, string workDir)
        {
            _workDir = workDir;
            _appType = appType;
            _repo = repo;

            _updateLogIO = new UpdateLogIO(workDir);
            TryLoadPendingUpdate();
        }


        public async Task InstallUpdate(string srcId, string versionId)
        {
            if (HasPendingUpdate)
                throw new Exception("Another update already executing");

            HasPendingUpdate = true;
            _state = new UpdateState();
            SaveUpdateState();
            OnStateChanged();

            try
            {
                var targetUpdate = _repo.UpdatesCache.FirstOrDefault(upd => upd.SrcId == srcId && upd.VersionId == versionId);
                if (targetUpdate == null)
                {
                    OnUpdateFailed("Update '{srcId}/{versionId}' not found");
                    return;
                }

                var toVersion = targetUpdate.Info.ReleaseVersion;
                FillUpdateParams(toVersion);
                _updateLogIO.LogUpdateInfo($"Updating to version '{toVersion}'");

                SetStatus("Downloading update...");
                string updateFilePath;
                try
                {
                    updateFilePath = await _repo.DownloadUpdate(srcId, versionId, GetAssetType());
                }
                catch (Exception ex)
                {
                    OnUpdateFailed("Failed to download update", ex);
                    return;
                }

                await InstallUpdateInternal(updateFilePath);
            }
            catch (Exception ex)
            {
                OnUpdateFailed("Update failed unexpectedly", ex);
            }
        }

        public async Task InstallUpdateFile(string updateFilePath)
        {
            if (HasPendingUpdate)
                throw new Exception("Another update already executing");

            HasPendingUpdate = true;
            _state = new UpdateState();
            SaveUpdateState();
            OnStateChanged();

            try
            {
                FillUpdateParams(null);
                _updateLogIO.LogUpdateInfo($"Installing update from '{updateFilePath}'");

                await InstallUpdateInternal(updateFilePath);
            }
            catch (Exception ex)
            {
                OnUpdateFailed("Update failed unexpectedly", ex);
            }
        }

        public void DiscardUpdateResult()
        {
            if (Status == UpdateStatusCodes.Pending)
                throw new Exception("Update is still executing");

            try
            {
                UpdateHelper.DiscardUpdateResult(_workDir);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to discard update result");
            }

            HasPendingUpdate = false;
            StatusDetails = null;
            UpdateLog = null;
            StateChangedCallback?.Invoke();
        }


        private void OnStateChanged()
        {
            UpdateLog = UpdateLogIO.TryReadLogOnce(_workDir);
            StateChangedCallback?.Invoke();
        }

        private void SaveUpdateState() => UpdateHelper.SaveUpdateState(_workDir, _state);

        private void SetStatus(string status, Exception err = null)
        {
            StatusDetails = status;
            if (err == null)
                _updateLogIO.LogUpdateStatus(status);
            else
                _updateLogIO.LogUpdateError(status, err);

            OnStateChanged();
        }

        private void OnUpdateFailed(string errMsg, Exception ex = null)
        {
            if (ex != null)
                _logger.Error(ex, errMsg);

            _updateLogIO.LogUpdateError(errMsg, ex);
            _state.SetError(errMsg);
            SaveUpdateState();
            StatusDetails = UpdateHelper.FormatStateError(_state);
            OnStateChanged();
        }

        private void FillUpdateParams(string toVersion)
        {
            var extractPath = Path.Combine(_workDir, "extract");
            _state.Params = new UpdateParams
            {
                AppTypeCode = (int)_appType,
                InstallPath = AppInfoProvider.Data.AppInfoFolder,
                UpdatePath = extractPath,
                FromVersion = AppVersionInfo.Current.Version,
                ToVersion = toVersion,
            };
            SaveUpdateState();
        }

        private UpdateAssetTypes GetAssetType()
        {
            return _appType switch
            {
                UpdateAppTypes.Terminal => UpdateAssetTypes.TerminalUpdate,
                UpdateAppTypes.Server => UpdateAssetTypes.ServerUpdate,
                _ => throw new NotSupportedException(),
            };
        }

        private async Task InstallUpdateInternal(string updateFilePath)
        {
            if (!File.Exists(updateFilePath))
            {
                OnUpdateFailed($"Update not found at '{updateFilePath}'");
                return;
            }

            SetStatus("Extracting files...");
            UpdateHelper.ExtractUpdate(updateFilePath, _state.Params.UpdatePath);

            SetStatus("Installing update...");
            var (startSuccess, startError) = await UpdateHelper.StartUpdate(_workDir, _state.Params);
            if (startSuccess)
            {
                _ = WatchPendingUpdateLoop();
            }
            else
            {
                _state = UpdateHelper.LoadUpdateState(_workDir);
                if (_state.HasErrors)
                {
                    StatusDetails = UpdateHelper.FormatStateError(_state);
                    OnStateChanged();
                }
                else
                {
                    OnUpdateFailed(startError ?? "Unexpected update error");
                }
            }
        }

        private async Task WatchPendingUpdateLoop()
        {
            var cancelTokenSrc = new CancellationTokenSource();
            lock (_syncObj)
            {
                _watchUpdateCancelTokenSrc?.Cancel();
                _watchUpdateCancelTokenSrc = cancelTokenSrc;
            }
            var cancelToken = cancelTokenSrc.Token;

            var loopTimeout = TimeSpan.FromSeconds(10);

            var startTime = DateTime.UtcNow;
            while (!cancelToken.IsCancellationRequested && _state.Status == UpdateStatusCodes.Pending)
            {
                await Task.Delay(loopTimeout, cancelToken);

                if (_state.Status == UpdateStatusCodes.Pending)
                {
                    _state = UpdateHelper.LoadUpdateState(_workDir);
                    if (_state.Status == UpdateStatusCodes.Pending && DateTime.UtcNow - startTime > PendingUpdateTimeout)
                    {
                        OnUpdateFailed("Update timeout reached");
                    }
                    else
                    {
                        ProcessPendingUpdateStatus();
                        OnStateChanged();
                    }
                }
            }
        }

        private void TryLoadPendingUpdate()
        {
            if (!UpdateHelper.IsUpdatePending(_workDir))
                return;

            try
            {
                UpdateLog = UpdateLogIO.TryReadLog(_workDir);
                _state = UpdateHelper.LoadUpdateState(_workDir);
                HasPendingUpdate = true;
                ProcessPendingUpdateStatus();

                _ = WatchPendingUpdateLoop();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to init pending update");
            }
        }

        private void ProcessPendingUpdateStatus()
        {
            try
            {
                StatusDetails = _state.Status switch
                {
                    UpdateStatusCodes.Pending => "Updating...",
                    UpdateStatusCodes.Completed => "Update success",
                    _ => _state.HasErrors ? UpdateHelper.FormatStateError(_state) : "Unknown update error",
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to process pending update state");
            }
        }
    }
}
