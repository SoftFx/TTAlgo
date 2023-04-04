using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using TickTrader.Algo.AppCommon;
using TickTrader.Algo.AppCommon.Update;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Updater
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var ctx = new UpdateContext();
            if (ctx.HasError)
            {
                // log error
                Environment.Exit((int)ctx.ErrorCode);
                return;
            }

            var processesToStop = GetAllProcessesForFolder("TickTrader.Algo", ctx.CurrentBinFolder);

            Console.WriteLine("Waiting for app to stop...");
            switch (ctx.AppType)
            {
                case UpdateAppTypes.Terminal:
                    if (processesToStop.Count != 0)
                    {
                        var cnt = 0;
                        while (processesToStop.Any(p => !p.HasExited))
                        {
                            if (cnt % 50 == 0)
                            {
                                var msgBuilder = new StringBuilder();
                                msgBuilder.AppendLine("Waiting for stop of process: ");
                                foreach (var p in processesToStop.Where(p => !p.HasExited)) msgBuilder.AppendLine($"{p.ProcessName} ({p.Id})");
                                Console.WriteLine(msgBuilder.ToString());
                            }
                            cnt++;
                            Thread.Sleep(100);
                        }
                    }
                    break;
                case UpdateAppTypes.Server:
                    // Wait for StopService. ServiceId - ?
                    break;
            }
            Console.WriteLine("App stopped");

            var rollbackFolder = InstallPathHelper.GetRollbackVersionFolder(ctx.InstallPath);
            if (Directory.Exists(rollbackFolder))
            {
                Console.WriteLine("Removing old rollback version...");
                Directory.Delete(rollbackFolder, true);
            }

            Directory.Move(ctx.CurrentBinFolder, rollbackFolder);
            Console.WriteLine("Moved current version to rollback");

            var copySuccess = true;
            try
            {
                Console.WriteLine("Copying new version files...");
                PathHelper.CopyDirectory(ctx.UpdateBinFolder, ctx.CurrentBinFolder, true, false);
                Console.WriteLine("Finished copying new files");
            }
            catch (Exception ex)
            {
                copySuccess = false;
                Console.WriteLine(ex.Message);
                Console.WriteLine("Copy failed");
            }

            if (!copySuccess)
            {
                Console.WriteLine("Perfoming rollback...");
                Directory.Delete(ctx.CurrentBinFolder, true);
                Directory.Move(rollbackFolder, ctx.CurrentBinFolder);
                Console.WriteLine("Rollback finished");
            }

            StartApp(ctx);
        }


        private static void StartApp(UpdateContext ctx)
        {
            switch (ctx.AppType)
            {
                case UpdateAppTypes.Terminal:
                    var exePath = Path.Combine(ctx.CurrentBinFolder, ctx.ExeFileName);
                    Process.Start(new ProcessStartInfo(exePath) { UseShellExecute = true });
                    break;
                case UpdateAppTypes.Server:
                    // Start service. ServiceId - ?
                    break;
            }
        }

        private static List<Process> GetAllProcessesForFolder(string processNamePrefix, string folderPath)
            => !string.IsNullOrEmpty(processNamePrefix)
                ? Process.GetProcesses().Where(p => p.ProcessName.StartsWith(processNamePrefix) && IsProcessMainFileInFolder(p, folderPath)).ToList()
                : Process.GetProcesses().Where(p => p.Id != 0 && p.Id != 4 && IsProcessMainFileInFolder(p, folderPath)).ToList();

        private static bool IsProcessMainFileInFolder(Process p, string folderPath)
            => p.MainModule?.FileName?.StartsWith(folderPath, StringComparison.OrdinalIgnoreCase) ?? false;
    }
}