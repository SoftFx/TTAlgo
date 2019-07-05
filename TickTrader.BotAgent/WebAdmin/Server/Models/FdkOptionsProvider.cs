using Microsoft.Extensions.Configuration;
using TickTrader.Algo.Common.Model;
using TickTrader.BotAgent.BA;
using TickTrader.BotAgent.BA.Models;
using TickTrader.BotAgent.WebAdmin.Server.Extensions;

namespace TickTrader.BotAgent.WebAdmin.Server.Models
{
    public class FdkOptionsProvider : IFdkOptionsProvider
    {
        private readonly IConfiguration _config;


        public FdkOptionsProvider(IConfiguration config)
        {
            _config = config;
        }


        public ConnectionOptions GetConnectionOptions()
        {
            var fdkSettings = _config.GetFdkSettings();
            return new ConnectionOptions() { EnableLogs = fdkSettings.EnableLogs, LogsFolder = ServerModel.Environment.LogFolder, Type = AppType.BotAgent };
        }
    }
}
