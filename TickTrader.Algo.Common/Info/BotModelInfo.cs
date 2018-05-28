using TickTrader.Algo.Common.Model.Config;

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

        public PluginConfig Config { get; set; }

        public BotStates State { get; set; }

        public string FaultMessage { get; set; }


        public BotModelInfo()
        {
        }
    }
}
