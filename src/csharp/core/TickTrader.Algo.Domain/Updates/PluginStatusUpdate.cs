namespace TickTrader.Algo.Domain
{
    public partial class PluginStatusUpdate
    {
        public PluginStatusUpdate(string pluginId, string message)
        {
            PluginId = pluginId;
            Message = message;
        }
    }
}
