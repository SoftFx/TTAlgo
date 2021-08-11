namespace TickTrader.Algo.Server.PublicAPI
{
    public partial class StartPluginRequest
    {
        public StartPluginRequest(string pluginId)
        {
            PluginId = pluginId;
        }
    }


    public partial class StopPluginRequest
    {
        public StopPluginRequest(string pluginId)
        {
            PluginId = pluginId;
        }
    }


    public partial class AddPluginRequest
    {
        public AddPluginRequest(string accountId, PluginConfig config)
        {
            AccountId = accountId;
            Config = config;
        }
    }


    public partial class RemovePluginRequest
    {
        public RemovePluginRequest(string pluginId, bool cleanLog = false, bool cleanAlgoData = false)
        {
            PluginId = pluginId;
            CleanLog = cleanLog;
            CleanAlgoData = cleanAlgoData;
        }
    }


    public partial class ChangePluginConfigRequest
    {
        public ChangePluginConfigRequest(string pluginId, PluginConfig newConfig)
        {
            PluginId = pluginId;
            NewConfig = newConfig;
        }
    }
}
