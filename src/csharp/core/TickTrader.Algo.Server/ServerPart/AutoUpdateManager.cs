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

        private readonly AlgoServerPrivate _server;

        private AutoUpdateEnums.Types.ServiceStatus _status;
        private string _errorDetails, _targetVersion;
        private AutoUpdateService _updateSvc;
        private bool _started;


        public AutoUpdateManager(AlgoServerPrivate server)
        {
            _server = server;

            _status = AutoUpdateEnums.Types.ServiceStatus.Idle;
            _started = false;
        }


        public void Start()
        {
            if (_started)
                return;

            try
            {
                _updateSvc = new AutoUpdateService(_server.Env.UpdatesFolder);
                _updateSvc.AddSource(new UpdateDownloadSource { Name = AutoUpdateService.MainSourceName, Uri = AutoUpdateService.MainGithubRepo });
                _updateSvc.EnableAutoCheck();

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

        public ServerVersionInfo GetCurrentVersion() => new(AppVersionInfo.Current.Version, AppVersionInfo.Current.BuildDate);

        public async Task<ServerUpdateList> GetUpdateList(ServerUpdateListRequest request)
        {
            if (!_started)
                throw new NotSupportedException("Update service not started");

            var res = new ServerUpdateList();
            try
            {
                var updates = await _updateSvc.GetUpdates(request.Forced);
                foreach (var update in updates)
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

        public async Task<StartServerUpdateResponse> StartUpdate(StartServerUpdateRequest request)
        {
            if (!_started)
                throw new NotSupportedException("Update service not started");

            try
            {
                UpdateStatus(AutoUpdateEnums.Types.ServiceStatus.Loading);

                var updatePath = Path.Combine(_server.Env.UpdatesFolder, "update.zip");
                var updateDir = Path.Combine(_server.Env.UpdatesFolder, "update");
                var downloadSuccess = false;
                if (!string.IsNullOrEmpty(request.ReleaseId))
                    downloadSuccess = await DownloadReleaseById(request.ReleaseId, updateDir);
                else if (!string.IsNullOrEmpty(request.LocalPath))
                    downloadSuccess = await DownloadReleaseFromLocalPath(request.LocalPath, updateDir, request.OwnsLocalFile);

                if (downloadSuccess)
                {
                    UpdateStatus(AutoUpdateEnums.Types.ServiceStatus.Updating);
                    var updateParams = new UpdateParams
                    {
                        AppTypeCode = (int)UpdateAppTypes.Server,
                        InstallPath = AppInfoProvider.Data.AppInfoFolder,
                        UpdatePath = updateDir,
                        FromVersion = AppVersionInfo.Current.Version,
                    };
                    var startSuccess = await UpdateHelper.StartUpdate(_server.Env.UpdatesFolder, updateParams, true);
                    if (!startSuccess)
                    {
                        var state = UpdateHelper.LoadUpdateState(_server.Env.UpdatesFolder);
                        var error = state.HasErrors ? UpdateHelper.FormatStateError(state) : "Unexpected update error";
                        UpdateStatus(AutoUpdateEnums.Types.ServiceStatus.UpdateFailed, error);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Update failed unexpectedly");
                UpdateStatus(AutoUpdateEnums.Types.ServiceStatus.UpdateFailed);
            }
            return new StartServerUpdateResponse { Status = GetStatus() };
        }


        private void UpdateStatus(AutoUpdateEnums.Types.ServiceStatus status, string errorDetails = null)
        {
            _status = status;
            _errorDetails = errorDetails;
        }

        private UpdateServiceStatusInfo GetStatus() => new(_status, _errorDetails, _targetVersion);


        private async Task<bool> DownloadReleaseById(string releaseId, string updateDir)
        {
            var cachePath = await _updateSvc.DownloadUpdate(AutoUpdateService.MainSourceName, releaseId, UpdateAssetTypes.ServerUpdate);

            UpdateHelper.ExtractUpdate(cachePath, updateDir);

            return true;
        }

        private Task<bool> DownloadReleaseFromLocalPath(string path, string updateDir, bool deleteFile = false)
        {
            UpdateHelper.ExtractUpdate(path, updateDir);

            if (deleteFile)
            {
                try
                {
                    File.Delete(path);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to delete temp update file");
                }
            }

            return Task.FromResult(true);
        }
    }
}
