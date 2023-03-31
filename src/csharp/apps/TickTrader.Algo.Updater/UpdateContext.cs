using System;
using System.Diagnostics;
using System.IO;
using TickTrader.Algo.AppCommon;
using TickTrader.Algo.AppCommon.Update;

namespace TickTrader.Algo.Updater
{
    public class UpdateContext
    {
        public UpdateErrorCodes ErrorCode { get; set; }

        public bool HasError => ErrorCode != UpdateErrorCodes.NoError;

        public Exception ErrorDetails { get; set; }

        public UpdateAppTypes AppType { get; set; }

        public string ExeFileName { get; set; }

        public string InstallPath { get; set; }

        public string CurrentBinFolder { get; set; }

        public string UpdateBinFolder { get; set; }

        public Process TargetProcess { get; set; }


        public UpdateContext()
        {
            try
            {
                ErrorCode = InitInternal();
            }
            catch (Exception ex)
            {
                ErrorDetails = ex;
                ErrorCode = UpdateErrorCodes.InitError;
            }
        }


        private UpdateErrorCodes InitInternal()
        {
            UpdaterParams updParams;
            try
            {
                updParams = UpdaterParams.ParseFromEnvVars(Environment.GetEnvironmentVariables());
            }
            catch (Exception ex)
            {
                ErrorDetails = ex;
                return UpdateErrorCodes.InitError;
            }
            if (updParams == null)
                return UpdateErrorCodes.InitError;

            if (!updParams.AppType.HasValue)
                return UpdateErrorCodes.InvalidAppType;
            AppType = updParams.AppType.Value;

            ExeFileName = UpdateHelper.GetAppExeFileName(AppType);
            if (string.IsNullOrEmpty(ExeFileName))
                return UpdateErrorCodes.UnexpectedAppType;

            if (!Directory.Exists(updParams.InstallPath))
                return UpdateErrorCodes.AppPathNotFound;
            InstallPath = updParams.InstallPath;

            CurrentBinFolder = InstallPathHelper.GetCurrentVersionFolder(InstallPath);
            if (!Directory.Exists(CurrentBinFolder))
                return UpdateErrorCodes.CurrentVersionNotFound;
            UpdateBinFolder = UpdateHelper.GetUpdateBinFolder(AppDomain.CurrentDomain.BaseDirectory);
            if (!Directory.Exists(UpdateBinFolder))
                return UpdateErrorCodes.UpdateVersionNotFound;
            if (!File.Exists(Path.Combine(UpdateBinFolder, ExeFileName)))
                return UpdateErrorCodes.UpdateVersionMissingExe;

            try
            {
                var pid = updParams.ProcessId;
                if (pid.HasValue)
                    Process.GetProcessById(pid.Value);
            }
            catch
            {
                return UpdateErrorCodes.InvalidProcessId;
            }

            return UpdateErrorCodes.NoError;
        }
    }
}
