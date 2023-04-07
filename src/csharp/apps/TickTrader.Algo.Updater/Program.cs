using Serilog;
using System;
using TickTrader.Algo.AppCommon.Update;

namespace TickTrader.Algo.Updater
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File($"Logs/{DateTime.UtcNow:yyyy-MM-dd-hh-mm-ss}-update.log")
                .CreateLogger();


            var ctx = new UpdateContext();
            if (ctx.HasError)
            {
                Log.Error(ctx.ErrorDetails, $"Init failed with error code = {ctx.ErrorCode}");
                Environment.Exit((int)ctx.ErrorCode);
                return;
            }

            switch (ctx.AppType)
            {
                case UpdateAppTypes.Terminal:
                    UpdateWindowViewModel.Init(ctx);
                    App.Main();
                    break;
                case UpdateAppTypes.Server:
                    // start with no GUI
                    break;
            }
        }
    }
}