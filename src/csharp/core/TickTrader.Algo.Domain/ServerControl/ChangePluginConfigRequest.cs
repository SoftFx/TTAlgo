namespace TickTrader.Algo.Domain.ServerControl
{
    public partial class ChangePluginConfigRequest
    {
        public ChangePluginConfigRequest(string pluginId, PluginConfig newConfig)
        {
            PluginId = pluginId;
            NewConfig = newConfig;
        }
    }
}
