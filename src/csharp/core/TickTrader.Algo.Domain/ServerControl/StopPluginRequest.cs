namespace TickTrader.Algo.Domain.ServerControl
{
    public partial class StopPluginRequest
    {
        public StopPluginRequest(string pluginId)
        {
            PluginId = pluginId;
        }
    }
}
