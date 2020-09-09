using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Info
{
    public enum PluginStates
    {
        Stopped,
        Starting,
        Faulted,
        Running,
        Stopping,
        Broken,
        Reconnecting,
    }


    public class BotModelInfo
    {
        public string InstanceId { get; set; }

        public AccountKey Account { get; set; }

        public PluginStates State { get; set; }

        public string FaultMessage { get; set; }

        public PluginConfig Config { get; set; }

        public PluginDescriptor Descriptor { get; set; }


        public BotModelInfo()
        {
        }
    }
}
