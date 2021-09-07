using NLog;
using System;
using System.Runtime.Serialization;
using TickTrader.Algo.Domain;

namespace TickTrader.BotAgent.BA.Models
{
    [DataContract(Name = "tradeBot", Namespace = "")]
    public class TradeBotModel
    {
        private static readonly ILogger _log = LogManager.GetLogger(nameof(ServerModel));

        private PluginConfig _config;
        [DataMember(Name = "configuration")]
        private Algo.Core.Config.PluginConfig _configEntry;


        public PluginConfig Config
        {
            get => _config;
            private set
            {
                _config = value;
                _configEntry = Algo.Core.Config.PluginConfig.FromDomain(value);
            }
        }
        [DataMember(Name = "running")]
        public bool IsRunning { get; private set; }

        public bool OnDeserialized()
        {
            try
            {
                _config = _configEntry.ToDomain();

                return true;
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"Failed to deserialize bot config {_configEntry.InstanceId}");
            }

            return false;
        }
    }
}
