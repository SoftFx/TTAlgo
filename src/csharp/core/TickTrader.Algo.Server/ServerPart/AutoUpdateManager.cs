using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using TickTrader.Algo.AppCommon;
using TickTrader.Algo.AppCommon.Update;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.Server
{
    internal class AutoUpdateManager
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<AutoUpdateManager>();

        private readonly AlgoServerPrivate _server;

        private AutoUpdateEnums.Types.ServiceStatus _status;


        public AutoUpdateManager(AlgoServerPrivate server)
        {
            _server = server;

            _status = AutoUpdateEnums.Types.ServiceStatus.Idle;
        }


        public string GetCurrentVersion() => AppVersionInfo.Current.Version;

        public async Task<StartServerUpdateResponse> StartUpdate(StartServerUpdateRequest request)
        {
            try
            {
                _status = AutoUpdateEnums.Types.ServiceStatus.Loading;
                
                var updatePath = Path.Combine(_server.Env.UpdatesFolder, "update.zip");
                var updateDir = Path.Combine(_server.Env.UpdatesFolder, "update");
                var downloadSuccess = false;
                if (!string.IsNullOrEmpty(request.DownloadUrl))
                    downloadSuccess = await DownloadUpdateFromUrl(request.DownloadUrl, updatePath, updateDir);

                if (downloadSuccess)
                {
                    _status = AutoUpdateEnums.Types.ServiceStatus.Updating;
                    var updateParams = new UpdateParams
                    {
                        AppTypeCode = (int)UpdateAppTypes.Server,
                        InstallPath = AppInfoProvider.Data.AppInfoFolder,
                        UpdatePath = updateDir,
                        FromVersion = AppVersionInfo.Current.Version,
                        //ToVersion = update.Version,
                    };
                    var startSuccess = await UpdateHelper.StartUpdate(_server.Env.UpdatesFolder, updateParams, true);
                    if (!startSuccess)
                    {
                        var state = UpdateHelper.LoadUpdateState(_server.Env.UpdatesFolder);
                        var error = state.HasErrors ? UpdateHelper.FormatStateError(state) : "Unexpected update error";
                        _status = AutoUpdateEnums.Types.ServiceStatus.UpdateFailed;

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Update failed unexpectedly");
                _status = AutoUpdateEnums.Types.ServiceStatus.UpdateFailed;
            }
            return new StartServerUpdateResponse { Status = _status };
        }


        private async Task<bool> DownloadUpdateFromUrl(string url, string updatePath, string updateDir)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _status = AutoUpdateEnums.Types.ServiceStatus.UpdateFailed;
                _logger.Error($"Download failed. Status code {response.StatusCode}");
                return false;
            }

            using (var file = File.Open(updatePath, FileMode.Create, FileAccess.Write))
            {
                await response.Content.CopyToAsync(file);
            }

            if (Directory.Exists(updateDir))
                Directory.Delete(updateDir, true);
            using (var file = File.Open(updatePath, FileMode.Open, FileAccess.Read))
            using (var zip = new ZipArchive(file))
            {
                zip.ExtractToDirectory(updateDir);
            }

            return true;
        }
    }
}
