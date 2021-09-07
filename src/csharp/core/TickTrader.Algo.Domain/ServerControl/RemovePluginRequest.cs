namespace TickTrader.Algo.Domain.ServerControl
{
    public partial class RemovePluginRequest
    {
        public RemovePluginRequest(string pluginId, bool cleanLog = false, bool cleanAlgoData = false)
        {
            PluginId = pluginId;
            CleanLog = cleanLog;
            CleanAlgoData = cleanAlgoData;
        }
    }
}
