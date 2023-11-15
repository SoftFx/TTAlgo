using System;
using System.IO;
using TickTrader.Algo.AppCommon.Update;

namespace TickTrader.Algo.Updater
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var ctx = new UpdateContext(Directory.GetCurrentDirectory());
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