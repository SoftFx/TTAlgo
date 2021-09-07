namespace TickTrader.Algo.Domain.ServerControl
{
    public partial class StartPluginRequest
    {
        public StartPluginRequest(string pluginId)
        {
            PluginId = pluginId;
        }
    }
}
