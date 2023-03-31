using System;
using System.Diagnostics;
using System.IO;
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

            Console.WriteLine("Waiting for app to stop...");
            switch (ctx.AppType)
            {
                case UpdateAppTypes.Terminal:
                    if (ctx.TargetProcess != null)
                    {
                        ctx.TargetProcess.WaitForExit(UpdateHelper.ShutdownTimeout);
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
            Console.WriteLine("Copying new version files...");
            PathHelper.CopyDirectory(ctx.UpdateBinFolder, ctx.CurrentBinFolder, true, false);
            Console.WriteLine("Finished copying new files");

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
    }
}