using System;
using System.IO;
using System.Threading.Tasks;
using TickTrader.Algo.AppCommon;
using TickTrader.Algo.AppCommon.Update;
using TickTrader.Algo.AutoUpdate;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.Server
{
    internal class AutoUpdateManager
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<AutoUpdateManager>();
        private static readonly ServerVersionInfo _currentVersion = new(AppVersionInfo.Current.Version, AppVersionInfo.Current.BuildDate);

        private readonly AlgoServerPrivate _server;
        private readonly string _updateWorkDir;

        private AutoUpdateEnums.Types.ServiceStatus _status;
        private string _statusDetails;
        private AutoUpdateService _updateSvc;
        private bool _started, _hasPendingUpdate;


        public AutoUpdateManager(AlgoServerPrivate server)
        {
            _server = server;
            _updateWorkDir = _server.Env.UpdatesFolder;

            _status = AutoUpdateEnums.Types.ServiceStatus.Idle;
            _started = false;
        }


        public void Start()
        {
            if (_started)
                return;

            try
            {
                _updateSvc = new AutoUpdateService(_updateWorkDir, UpdateAppTypes.Server);
                _updateSvc.SetNewVersionCallback(OnNewVersionAvailable, true); // callback should return to actor context
                _updateSvc.EnableAutoCheck();
                _updateSvc.SetUpdateStateChangedCallback(OnUpdateStateChanged, true); // callback should return to actor context

                OnUpdateStateChanged(); // load current state

                _started = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to start auto update service");
            }
        }

        public void Stop()
        {
            if (!_started)
                return;

            try
            {
                _updateSvc.DisableAutoCheck();
                _updateSvc = null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to stop auto update service");
            }
        }

        public ServerVersionInfo GetCurrentVersion() => _currentVersion;

        public async Task<ServerUpdateList> GetUpdateList(ServerUpdateListRequest request)
        {
            if (!_started)
                throw new NotSupportedException("Update service not started");

            var res = new ServerUpdateList();
            try
            {
                var updateList = await _updateSvc.GetUpdates(request.Forced);
                res.Errors.AddRange(updateList.Errors);
                foreach (var update in updateList.Updates)
                {
                    if (update.AvailableAssets.Contains(UpdateAssetTypes.ServerUpdate))
                    {
                        var updInfo = new ServerUpdateInfo
                        {
                            ReleaseId = update.VersionId,
                            Version = update.Info.ReleaseVersion,
                            ReleaseDate = update.Info.ReleaseDate,
                            MinVersion = update.Info.MinVersion,
                            Changelog = update.Info.Changelog,
                            IsStable = update.IsStable,
                        };
                        res.Updates.Add(updInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to fetch updates. Request: {request}");
            }
            return res;
        }

        public StartServerUpdateResponse StartUpdate(StartServerUpdateRequest request)
        {
            if (!_started)
                throw new NotSupportedException("Update service not started");
            if (_hasPendingUpdate)
                return new StartServerUpdateResponse { Started = false, ErrorMsg = "Another update already executing" };

            _hasPendingUpdate = true;

            if (!string.IsNullOrEmpty(request.LocalPath))
            {
                // update files loaded into TEMP are deleted after receiving response
                var tmpUpdateFile = Path.Combine(_updateWorkDir, "update-tmp.zip");
                if (File.Exists(tmpUpdateFile))
                    File.Delete(tmpUpdateFile);
                File.Copy(request.LocalPath, tmpUpdateFile);
                request = new StartServerUpdateRequest { LocalPath = tmpUpdateFile };
            }

            _ = ExecUpdate(request);
            return new StartServerUpdateResponse { Started = true };
        }

        public void DiscardUpdateResult(DiscardServerUpdateResultRequest request)
        {
            if (!_started)
                throw new NotSupportedException("Update service not started");

            _updateSvc.DiscardUpdateResult();
        }


        private async Task ExecUpdate(StartServerUpdateRequest request)
        {
            // Reschedule method to current SynchronizationContext
            await Task.Yield();

            try
            {
                if (!string.IsNullOrEmpty(request.ReleaseId))
                    await _updateSvc.InstallUpdate(AutoUpdateService.MainSourceName, request.ReleaseId);
                else if (!string.IsNullOrEmpty(request.LocalPath))
                    await _updateSvc.InstallUpdateFile(request.LocalPath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Update failed unexpectedly");
                UpdateStatus(AutoUpdateEnums.Types.ServiceStatus.UpdateFailed, "Update failed unexpectedly");
            }
        }

        private void UpdateStatus(AutoUpdateEnums.Types.ServiceStatus status, string statusDetails = null)
        {
            _status = status;
            _statusDetails = statusDetails;
            SendStatusUpdate();
        }

        private void SendStatusUpdate()
        {
            var snapshot = new UpdateServiceInfo(_status, _statusDetails)
            {
                UpdateLog = _updateSvc.UpdateLog,
                HasNewVersion = _updateSvc.HasNewVersion,
                NewVersion = _updateSvc.NewVersion,
            };
            _server.SendUpdate(new UpdateServiceStateUpdate { Snapshot = snapshot });
        }

        private void OnNewVersionAvailable() => SendStatusUpdate();

        private void OnUpdateStateChanged()
        {
            UpdateStatus();
            SendStatusUpdate();
        }

        private void UpdateStatus()
        {
            if (_updateSvc.HasPendingUpdate)
            {
                _hasPendingUpdate = true;
                _status = _updateSvc.UpdateStatus switch
                {
                    UpdateStatusCodes.Pending => AutoUpdateEnums.Types.ServiceStatus.Updating,
                    UpdateStatusCodes.Completed => AutoUpdateEnums.Types.ServiceStatus.UpdateSuccess,
                    _ => AutoUpdateEnums.Types.ServiceStatus.UpdateFailed,
                };
                _statusDetails = _updateSvc.UpdateStatusDetails;
            }
            else
            {
                _hasPendingUpdate = false;
                _status = AutoUpdateEnums.Types.ServiceStatus.Idle;
                _statusDetails = null;
            }
        }
    }
}
