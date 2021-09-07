namespace TickTrader.Algo.Domain
{
    public partial class PluginStateUpdate
    {
        public PluginStateUpdate(string id, PluginModelInfo.Types.PluginState state, string faultMessage)
        {
            Id = id;
            State = state;
            FaultMessage = faultMessage;
        }
    }
}
