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
                .WriteTo.File(UpdateHelper.LogFileName)
                .CreateLogger();

            var ctx = new UpdateContext();
            if (ctx.HasError)
            {
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
                    ctx.RunUpdateAsync().Wait();
                    break;
            }
        }
    }
}