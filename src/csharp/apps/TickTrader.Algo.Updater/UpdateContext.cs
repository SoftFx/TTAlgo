using Microsoft.Win32;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.AppCommon;
using TickTrader.Algo.AppCommon.Update;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Updater
{
    public interface IUpdateObserver
    {
        void OnStatusUpdated(string msg);

        bool KillQuestionCallback(string msg);

        void OnCompleted();
    }


    public class UpdateContext
    {
        private const int KillQuestionDelayCnt = 100;
        private const int SuccessStartTimeout = 10000;
        private const int ServiceStopTimeout = 90000;

        private readonly string _workDir = Directory.GetCurrentDirectory();


        public UpdateErrorCodes ErrorCode { get; private set; }

        public bool HasError => ErrorCode != UpdateErrorCodes.NoError;

        public UpdateState State { get; private set; }

        public UpdateAppTypes AppType { get; private set; }

        public string ExeFileName { get; private set; }

        public string InstallPath { get; private set; }

        public string CurrentBinFolder { get; private set; }

        public string UpdateBinFolder { get; private set; }

        public string RegistryId { get; private set; }

        public string ServiceId { get; private set; }

        public IUpdateObserver UpdateObserver { get; set; }


        public UpdateContext()
        {
            try
            {
                State = UpdateHelper.LoadUpdateState(_workDir);
            }
            catch (Exception ex)
            {
                ErrorCode = UpdateErrorCodes.InitError;
                Log.Error(ex, "Failed to load UpdateState");
            }

            if (!HasError && State != null)
            {
                try
                {
                    ErrorCode = InitInternal();
                    if (HasError)
                        LogError($"Init failed with error code = {ErrorCode}");
                }
                catch (Exception ex)
                {
                    ErrorCode = UpdateErrorCodes.InitError;
                    LogError("Init failed with generic error", ex);
                }

                if (HasError)
                {
                    State.InitErrorCode = (int)ErrorCode;
                    State.SetStatus(UpdateStatusCodes.InitFailed);
                    TrySaveState();
                }
            }
        }


        public Task RunUpdateAsync() => Task.Run(() => RunUpdate());


        private UpdateErrorCodes InitInternal()
        {
            if (!OperatingSystem.IsWindows())
                return UpdateErrorCodes.PlatformNotSupported;

            var updParams = State.Params;
            if (updParams == null)
                return UpdateErrorCodes.MissingParams;

            if (State.Status != UpdateStatusCodes.Pending)
                return UpdateErrorCodes.IncorrectStatus;

            AppType = updParams.AppType;
            ExeFileName = UpdateHelper.GetAppExeFileName(AppType);
            if (string.IsNullOrEmpty(ExeFileName))
                return UpdateErrorCodes.UnexpectedAppType;

            if (!Directory.Exists(updParams.InstallPath))
                return UpdateErrorCodes.AppPathNotFound;
            InstallPath = updParams.InstallPath;

            CurrentBinFolder = InstallPathHelper.GetCurrentVersionFolder(InstallPath);
            if (!Directory.Exists(CurrentBinFolder))
                return UpdateErrorCodes.CurrentVersionNotFound;
            var updatePath = !string.IsNullOrEmpty(updParams.UpdatePath) ? updParams.UpdatePath : AppDomain.CurrentDomain.BaseDirectory;
            UpdateBinFolder = UpdateHelper.GetUpdateBinFolder(updatePath);
            if (!Directory.Exists(UpdateBinFolder))
                return UpdateErrorCodes.UpdateVersionNotFound;
            if (!File.Exists(Path.Combine(UpdateBinFolder, ExeFileName)))
                return UpdateErrorCodes.UpdateVersionMissingExe;

            RegistryId = ResolveRegistryId(AppType, InstallPath);
            if (string.IsNullOrEmpty(RegistryId))
                return UpdateErrorCodes.RegistryIdNotFound;
            if (AppType == UpdateAppTypes.Server)
            {
                ServiceId = GetServiceIdFromRegisty(RegistryId);
                if (string.IsNullOrEmpty(ServiceId))
                    return UpdateErrorCodes.ServiceIdNotFound;
            }

            return UpdateErrorCodes.NoError;
        }

        private void TrySaveState()
        {
            try
            {
                UpdateHelper.SaveUpdateState(_workDir, State);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save UpdateState");
            }
        }

        private async Task RunUpdate()
        {
            try
            {
                await RunUpdateInternal();
            }
            catch (Exception ex)
            {
                LogError("Unexpected update error", ex);
                State.SetStatus(UpdateStatusCodes.UpdateError);
            }

            TrySaveState();
            UpdateObserver?.OnCompleted();
        }

        private async Task RunUpdateInternal()
        {
            UpdateStatus("Waiting for app to stop...");
            var stopSuccess = await StopApp();
            if (!stopSuccess)
            {
                State.SetStatus(UpdateStatusCodes.UpdateError);
                UpdateStatus("Starting old version...");
                _ = await StartApp();
                return;
            }
            UpdateStatus("App stopped");

            var rollbackFolder = InstallPathHelper.GetRollbackVersionFolder(InstallPath);
            if (Directory.Exists(rollbackFolder))
            {
                UpdateStatus("Removing old rollback version...");
                await SafeFSAction("Remove rollback version", () => Directory.Delete(rollbackFolder, true));
            }

            await SafeFSAction("Move version current->rollback", () => Directory.Move(CurrentBinFolder, rollbackFolder));
            UpdateStatus("Moved current version to rollback");

            var copySuccess = false;
            try
            {
                UpdateStatus("Moving new version files...");
#if DEBUG
                PathHelper.CopyDirectory(UpdateBinFolder, CurrentBinFolder, true, false);
#else
                Directory.Move(UpdateBinFolder, CurrentBinFolder);
#endif
                UpdateStatus("Finished moving new files");
                copySuccess = true;
            }
            catch (Exception ex)
            {
                LogError("Copy new version failed", ex);
            }

            var startSuccess = false;
            if (copySuccess)
            {
                UpdateStatus("Starting new version...");
                startSuccess = await StartApp();
            }

            if (startSuccess)
                State.SetStatus(UpdateStatusCodes.Completed);

            if (!copySuccess || !startSuccess)
            {
                try
                {
                    UpdateStatus("Perfoming rollback...");
                    await SafeFSAction("Delete new version", () => Directory.Delete(CurrentBinFolder, true));
                    await SafeFSAction("Restore version from rollback", () => Directory.Move(rollbackFolder, CurrentBinFolder));
                    UpdateStatus("Rollback finished");

                    UpdateStatus("Starting rollback version...");
                    var rollbackStarted = await StartApp();
                    State.SetStatus(rollbackStarted ? UpdateStatusCodes.RollbackCompleted : UpdateStatusCodes.RollbackFailed);
                }
                catch (Exception ex)
                {
                    LogError("Rollback failed", ex);
                    State.SetStatus(UpdateStatusCodes.RollbackFailed);
                }
            }
        }

        private async Task<bool> StopApp()
        {
            try
            {
                var processesToStop = GetAllProcessesForFolder("TickTrader.Algo", CurrentBinFolder);

                var stopTask = AppType switch
                {
                    UpdateAppTypes.Terminal => StopTerminal(processesToStop),
                    UpdateAppTypes.Server => StopServer(processesToStop),
                    _ => throw new ArgumentException($"Invalid AppType: {AppType}"),
                };
                await stopTask;
                return true;
            }
            catch (Exception ex)
            {
                LogError("StopApp failed", ex);
                return false;
            }
        }

        private async Task StopTerminal(List<Process> processesToStop)
        {
            if (processesToStop.Count != 0)
            {
                var cnt = 0;
                while (processesToStop.Any(p => !p.HasExited))
                {
                    cnt++;
                    await Task.Delay(100);
                    if (cnt % KillQuestionDelayCnt == 0 && UpdateObserver != null)
                    {
                        var msgBuilder = new StringBuilder();
                        msgBuilder.AppendLine("Waiting for stop of process: ");
                        foreach (var p in processesToStop.Where(p => !p.HasExited)) msgBuilder.AppendLine($"{p.ProcessName} ({p.Id})");
                        msgBuilder.AppendLine("Kill now?");

                        var killNowFlag = UpdateObserver.KillQuestionCallback(msgBuilder.ToString());
                        if (killNowFlag)
                        {
                            foreach (var p in processesToStop)
                            {
                                if (!p.HasExited)
                                    p.Kill();
                            }
                        }
                    }
                }
            }
        }

        private async Task StopServer(List<Process> processesToStop)
        {
            // Give server some time to return response
            await Task.Delay(UpdateHelper.UpdateFailTimeout + 2_000);

            var serviceStopped = true;
            var svcControl = new ServiceController(ServiceId);
            if (svcControl.Status != ServiceControllerStatus.Stopped)
            {
                serviceStopped = false;
                try
                {
                    svcControl.Stop();
                    svcControl.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMilliseconds(ServiceStopTimeout));
                    serviceStopped = true;
                }
                catch (System.ServiceProcess.TimeoutException)
                {
                    LogError("Service failed to stop within timeout");
                }
            }

            foreach (var p in processesToStop)
            {
                if (!p.HasExited)
                    p.Kill();
            }

            if (!serviceStopped)
                throw new Exception("Abort update");
        }

        private async Task<bool> StartApp()
        {
            try
            {
                var startTask = AppType switch
                {
                    UpdateAppTypes.Terminal => StartTerminal(),
                    UpdateAppTypes.Server => StartServer(),
                    _ => throw new ArgumentException($"Invalid AppType: {AppType}"),
                };
                return await startTask;
            }
            catch (Exception ex)
            {
                LogError("StartApp failed", ex);
                return false;
            }
        }

        private async Task<bool> StartTerminal()
        {
            var exePath = Path.Combine(CurrentBinFolder, ExeFileName);
            var p = Process.Start(new ProcessStartInfo(exePath) { UseShellExecute = true });
            await Task.WhenAny(p.WaitForExitAsync(), Task.Delay(SuccessStartTimeout));
            if (p.HasExited && p.ExitCode != 0)
            {
                UpdateStatus($"Start failed. Exit code: {p.ExitCode}");
                return false;
            }
            return true;
        }

        private async Task<bool> StartServer()
        {
            var svcControl = new ServiceController(ServiceId);
            if (svcControl.Status != ServiceControllerStatus.Stopped)
            {
                LogError($"Unexpected service state: {svcControl.Status}");
                return false;
            }

            svcControl.Start();
            svcControl.WaitForStatus(ServiceControllerStatus.Running);

            await Task.Delay(SuccessStartTimeout);
            if (svcControl.Status != ServiceControllerStatus.Running)
            {
                LogError($"Start failed: Service status: {svcControl.Status}");
            }

            return true;
        }

        private void UpdateStatus(string msg)
        {
            Log.Information(msg);
            UpdateObserver?.OnStatusUpdated(msg);
        }

        private void LogError(string msg, Exception ex = null)
        {
            Log.Error(ex, msg);
            State?.UpdateErrors?.Add(msg);
            UpdateObserver?.OnStatusUpdated(msg);
        }

        private async Task SafeFSAction(string actionName, Action action)
        {
            // Windows doesn't always releases files immediately after process is killed
            await Task.Delay(1000); // Initial delay
            Exception error = null;
            for (var i = 0; i < 3; i++)
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception ex)
                {
                    error = ex;
                    Log.Error(ex, actionName);
                }

                await Task.Delay(7000); // delay between attempts
            }
            throw error;
        }


        private static List<Process> GetAllProcessesForFolder(string processNamePrefix, string folderPath)
            => !string.IsNullOrEmpty(processNamePrefix)
                ? Process.GetProcesses().Where(p => p.ProcessName.StartsWith(processNamePrefix) && IsProcessMainFileInFolder(p, folderPath)).ToList()
                : Process.GetProcesses().Where(p => p.Id != 0 && p.Id != 4 && IsProcessMainFileInFolder(p, folderPath)).ToList();

        private static bool IsProcessMainFileInFolder(Process p, string folderPath)
            => p.MainModule?.FileName?.StartsWith(folderPath, StringComparison.OrdinalIgnoreCase) ?? false;

        private static string GetAppRegistryKeyName(UpdateAppTypes appType)
        {
            return appType switch
            {
                UpdateAppTypes.Terminal => "AlgoTerminal",
                UpdateAppTypes.Server => "AlgoServer",
                _ => string.Empty,
            };
        }

        private static string ResolveRegistryId(UpdateAppTypes appType, string installPath)
        {
            var appSubKeyName = GetAppRegistryKeyName(appType);
            if (string.IsNullOrEmpty(appSubKeyName))
                return null;

            var registry64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            var allInstallsKey = registry64.OpenSubKey(Path.Combine("SOFTWARE", "TickTrader", appSubKeyName));
            if (allInstallsKey == null)
                return null;

            foreach (var installId in allInstallsKey.GetSubKeyNames())
            {
                var installKey = allInstallsKey.OpenSubKey(installId);
                if (installKey != null)
                {
                    var registryInstallPath = installKey.GetValue("Path")?.ToString();
                    Path.GetFullPath(registryInstallPath);
                    // Windows paths are not case-sensitive
                    if (string.Equals(registryInstallPath, installPath, StringComparison.OrdinalIgnoreCase))
                        return installId;
                }
            }

            return null;
        }

        private static string GetServiceIdFromRegisty(string registyId)
        {
            var appSubKeyName = GetAppRegistryKeyName(UpdateAppTypes.Server);
            if (string.IsNullOrEmpty(appSubKeyName))
                return null;

            var registry64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            var serverKey = registry64.OpenSubKey(Path.Combine("SOFTWARE", "TickTrader", appSubKeyName, registyId));
            return serverKey?.GetValue("ServiceId")?.ToString();
        }
    }
}
