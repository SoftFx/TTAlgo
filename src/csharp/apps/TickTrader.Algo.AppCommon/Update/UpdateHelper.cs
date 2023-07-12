using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TickTrader.Algo.AppCommon.Update
{
    public static class UpdateHelper
    {
        public const string TerminalFileName = "TickTrader.AlgoTerminal.exe";
        public const string ServerFileName = "TickTrader.AlgoServer.exe";
        public const string StateFileName = "update-state.json";
        public const string LogFileName = "update.log";
        public const string InfoFileName = "update-info.json";
        public const int UpdateFailTimeout = 1_000;
        public const int ShutdownTimeout = 30_000;
        public const int UpdateHistoryMaxRecords = 5;

        private static readonly JsonSerializerOptions _stateSerializerOptions = new JsonSerializerOptions { IgnoreReadOnlyProperties = true, WriteIndented = true };


        public static string GetAppExeFileName(UpdateAppTypes appType)
        {
            return appType switch
            {
                UpdateAppTypes.Terminal => TerminalFileName,
                UpdateAppTypes.Server => ServerFileName,
                _ => string.Empty
            };
        }

        public static string GetUpdateBinFolder(string updatePath) => Path.Combine(updatePath, "update");


        public static void ExtractUpdate(string zipPath, string dstDir)
        {
            if (Directory.Exists(dstDir))
                Directory.Delete(dstDir, true);
            using (var file = File.Open(zipPath, FileMode.Open, FileAccess.Read))
            using (var zip = new ZipArchive(file))
            {
                zip.ExtractToDirectory(dstDir);
            }
        }

        public static async Task<bool> StartUpdate(string updateWorkDir, UpdateParams updateParams, bool useShellExecute)
        {
            CreateUpdateHistoryRecord(updateWorkDir);

            var updateState = new UpdateState { Params = updateParams };
            SaveUpdateState(updateWorkDir, updateState);

            var updInfo = LoadUpdateInfo(updateParams.UpdatePath);
            updateParams.ToVersion = updInfo.ReleaseVersion;
            var updateEntryPoint = Path.Combine(updateParams.UpdatePath, updInfo.Executable);
            var startInfo = new ProcessStartInfo(updateEntryPoint) { UseShellExecute = useShellExecute, WorkingDirectory = updateWorkDir };

            var proc = Process.Start(startInfo);
            await Task.WhenAny(Task.Delay(UpdateFailTimeout), proc.WaitForExitAsync()); // wait in case of any issues with update

            if (proc.HasExited)
                return false;

            return true;
        }

        public static UpdateState LoadUpdateState(string workDir)
        {
            var statePath = Path.Combine(workDir, StateFileName);
            using var file = File.Open(statePath, FileMode.Open, FileAccess.Read);
            return JsonSerializer.Deserialize<UpdateState>(file, _stateSerializerOptions);
        }

        public static void SaveUpdateState(string workDir, UpdateState state)
        {
            var statePath = Path.Combine(workDir, StateFileName);
            using var file = File.Open(statePath, FileMode.Create);
            JsonSerializer.Serialize(file, state, _stateSerializerOptions);
        }

        public static UpdateInfo LoadUpdateInfo(string dirPath)
        {
            var filePath = Path.Combine(dirPath, InfoFileName);
            using var file = File.Open(filePath, FileMode.Open, FileAccess.Read);
            return JsonSerializer.Deserialize<UpdateInfo>(file);
        }

        public static void SaveUpdateInfo(string dirPath, UpdateInfo info)
        {
            var filePath = Path.Combine(dirPath, InfoFileName);
            using var file = File.Open(filePath, FileMode.Create);
            JsonSerializer.Serialize(file, info);
        }

        public static string FormatStateError(UpdateState state)
        {
            var sb = new StringBuilder();

            sb.Append(state.Status.ToString());
            if (state.InitError != UpdateErrorCodes.NoError)
                sb.Append(" - ").Append(state.InitError.ToString());
            sb.AppendLine();
            foreach (var err in state.UpdateErrors)
                sb.AppendLine(err);

            return sb.ToString();
        }


        private static void CreateUpdateHistoryRecord(string updateWorkDir)
        {
            var statePath = Path.Combine(updateWorkDir, StateFileName);
            var logPath = Path.Combine(updateWorkDir, LogFileName);

            if (File.Exists(statePath))
            {
                var files = Directory.GetFiles(updateWorkDir, "UpdateHistory*.zip");
                if (files.Length >= UpdateHistoryMaxRecords)
                {
                    // Cleanup old files
                    Array.Sort(files, static (x, y) => File.GetCreationTimeUtc(x).CompareTo(File.GetCreationTimeUtc(y)));
                    for (var i = 0; files.Length - i >= UpdateHistoryMaxRecords; i++)
                    {
                        try
                        {
                            File.Delete(files[i]);
                        }
                        catch (Exception) { }
                    }
                }

                var historyFilePath = Path.Combine(updateWorkDir, $"UpdateHistory-{File.GetCreationTimeUtc(statePath):yyyy-MM-dd-hh-mm-ss}.zip");
                using (var archiveStream = new FileStream(historyFilePath, FileMode.Create))
                using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(statePath, StateFileName);
                    File.Delete(statePath);

                    if (File.Exists(logPath))
                    {
                        archive.CreateEntryFromFile(logPath, LogFileName);
                        File.Delete(logPath);
                    }
                }
            }
        }
    }
}
