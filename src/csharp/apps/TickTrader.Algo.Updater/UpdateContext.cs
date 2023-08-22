using Microsoft.Win32;
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
        private const int SuccessTerminalStartTimeout = 5000;
        private const int SuccessServerStartTimeout = 10000;
        private const int ServiceStopTimeout = 90000;
        private const int ServiceStartTimeout = 60000;

        private readonly string _workDir;
        private readonly UpdateLogIO _updateLogIO;


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


        public UpdateContext(string workDir)
        {
            _workDir = workDir;
            _updateLogIO = new UpdateLogIO(_workDir);

            try
            {
                State = UpdateHelper.LoadUpdateState(_workDir);
            }
            catch (Exception ex)
            {
                ErrorCode = UpdateErrorCodes.InitError;
                _updateLogIO.LogUpdateError("Failed to load UpdateState", ex);
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
                _updateLogIO.LogUpdateError("Failed to save UpdateState", ex);
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
            var newVersionFolder = InstallPathHelper.GetNewVersionFolder(InstallPath);
            if (Directory.Exists(newVersionFolder))
            {
                UpdateStatus("Cleaning new version folder...");
                await SafeFSAction("Clean new version folder", () => Directory.Delete(newVersionFolder, true));
            }

            try
            {
                UpdateStatus("Copying new version files...");
                PathHelper.CopyDirectory(UpdateBinFolder, newVersionFolder, true, false);
                UpdateStatus("Finished copying new version files");
            }
            catch (Exception ex)
            {
                LogError("Copy new version failed", ex);
                State.SetStatus(UpdateStatusCodes.UpdateError);
                return;
            }

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
                Directory.Move(newVersionFolder, CurrentBinFolder);
                copySuccess = true;
            }
            catch (Exception ex)
            {
                LogError("Move new version failed", ex);
            }

            var startSuccess = false;
            if (copySuccess)
            {
                UpdateStatus("Starting new version...");
                startSuccess = await StartApp();
            }

            if (startSuccess)
            {
                State.SetStatus(UpdateStatusCodes.Completed);
                UpdateStatus("New version installed successfully");
                try
                {
                    if (Directory.Exists(rollbackFolder))
                    {
                        UpdateStatus("Cleaning rollback version...");
                        Directory.Delete(rollbackFolder, true);
                    }
                }
                catch (Exception ex)
                {
                    LogError("Failed to cleanup rollback version", ex);
                    // Non fatal
                }
            }
            else if (!copySuccess || !startSuccess)
            {
                try
                {
                    UpdateStatus("Perfoming rollback...");
                    KillApp(); // kill everything that is working at 'bin/current' for some reason
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
                var stopTask = AppType switch
                {
                    UpdateAppTypes.Terminal => StopTerminal(),
                    UpdateAppTypes.Server => StopServer(),
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

        private void KillApp(List<Process> processesToStop = null)
        {
            if (processesToStop == null)
                processesToStop = GetAllProcessesForFolder("TickTrader.Algo", CurrentBinFolder);

            foreach (var p in processesToStop)
            {
                if (!p.HasExited)
                    p.Kill();
            }
        }

        private async Task StopTerminal()
        {
            var processesToStop = GetAllProcessesForFolder("TickTrader.Algo", CurrentBinFolder);
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
                            KillApp(processesToStop);
                    }
                }
            }
        }

        private async Task StopServer()
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

            KillApp();

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
            if (p.HasExited && p.ExitCode != 0)
            {
                UpdateStatus($"Start failed. Exit code: {p.ExitCode}");
                return false;
            }
            UpdateStatus("New version started. Waiting for unexpected stop...");
            await Task.WhenAny(p.WaitForExitAsync(), Task.Delay(SuccessTerminalStartTimeout));
            if (p.HasExited && p.ExitCode != 0)
            {
                UpdateStatus($"New version stopped unexpectedly. Exit code: {p.ExitCode}");
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

            try
            {
                TryStartService(svcControl, TimeSpan.FromMilliseconds(ServiceStartTimeout));
            }
            catch (System.ServiceProcess.TimeoutException)
            {
                LogError($"Service failed to start within timeout. Service status: {svcControl.Status}");
                return false;
            }

            if (svcControl.Status != ServiceControllerStatus.Running)
            {
                LogError($"Start failed. Service status: {svcControl.Status}");
                return false;
            }
            else
            {
                UpdateStatus("New version started. Waiting for unexpected stop...");
                await Task.Delay(SuccessServerStartTimeout);
                if (svcControl.Status != ServiceControllerStatus.Running)
                {
                    LogError($"New version stopped unexpectedly. Service status: {svcControl.Status}");
                    return false;
                }
            }

            return true;
        }

        private void UpdateStatus(string msg)
        {
            _updateLogIO.LogUpdateStatus(msg);
            UpdateObserver?.OnStatusUpdated(msg);
        }

        private void LogError(string msg, Exception ex = null)
        {
            _updateLogIO.LogUpdateError(msg, ex);
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
                    _updateLogIO.LogUpdateError(actionName, ex);
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

            installPath = Path.TrimEndingDirectorySeparator(installPath);
            foreach (var installId in allInstallsKey.GetSubKeyNames())
            {
                var installKey = allInstallsKey.OpenSubKey(installId);
                if (installKey != null)
                {
                    var registryInstallPath = installKey.GetValue("Path")?.ToString();
                    registryInstallPath = Path.TrimEndingDirectorySeparator(registryInstallPath);
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

        private static bool IsFiniteServiceStatus(ServiceControllerStatus status)
        {
            return status switch
            {
                ServiceControllerStatus.Running => true,
                ServiceControllerStatus.Stopped => true,
                ServiceControllerStatus.Paused => true,
                _ => false,
            };
        }

        private static bool TryStartService(ServiceController svcControl, TimeSpan timeout)
        {
            // Service status changes seem to be delayed.
            // Checking ServiceController.Status right after calling Start() returns Stopped.
            // Liooks like ServiceController.WaitForStatus() handles this problem some way, but we need oneof (Running, Stopped)
            svcControl.Start();
            var startTime = DateTime.UtcNow;
            while (true)
            {
                try
                {
                    svcControl.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(3));
                }
                catch (System.ServiceProcess.TimeoutException) { }

                if (IsFiniteServiceStatus(svcControl.Status))
                    return svcControl.Status == ServiceControllerStatus.Running;

                var timePassed = DateTime.UtcNow - startTime;
                if (timePassed > timeout)
                    throw new System.ServiceProcess.TimeoutException();
            }
        }
    }
}
