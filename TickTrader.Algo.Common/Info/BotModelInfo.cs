using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Info
{
    public enum BotStates
    {
        Offline,
        Starting,
        Faulted,
        Online,
        Stopping,
        Broken,
        Reconnecting,
    }


    public class BotModelInfo
    {
        public string InstanceId { get; set; }

        public AccountKey Account { get; set; }

        public BotStates State { get; set; }

        public string FaultMessage { get; set; }

        public PluginConfig Config { get; set; }

        public PluginDescriptor Descriptor { get; set; }


        public BotModelInfo()
        {
        }
    }
}
