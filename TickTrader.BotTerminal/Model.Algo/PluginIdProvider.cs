using System.Collections.Generic;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class PluginIdProvider : IPluginIdProvider
    {
        private BotIdHelper _pluginsIdHelper;
        private Dictionary<string, int> _plugins;


        public PluginIdProvider()
        {
            _plugins = new Dictionary<string, int>();
            _pluginsIdHelper = new BotIdHelper();
        }

        public string GeneratePluginId(PluginDescriptor descriptor)
        {
            switch (descriptor.Type)
            {
                case Metadata.Types.PluginType.Indicator:
                case Metadata.Types.PluginType.TradeBot:
                    return GeneratePluginId(descriptor.DisplayName);
                default:
                    return GenerateId(descriptor.DisplayName);
            }
        }

        public bool IsValidPluginId(Metadata.Types.PluginType pluginType, string pluginId)
        {
            if (!_pluginsIdHelper.Validate(pluginId))
            {
                return false;
            }

            return pluginType == Metadata.Types.PluginType.Indicator || pluginType == Metadata.Types.PluginType.TradeBot ? !_plugins.ContainsKey(pluginId) : true;
        }

        public void RegisterPluginId(string pluginId)
        {
            _plugins.Add(pluginId, 1);
        }

        public void UnregisterPlugin(string pluginId)
        {
            if (_plugins.ContainsKey(pluginId))
            {
                _plugins.Remove(pluginId);
            }
        }

        public void Reset()
        {
            _plugins.Clear();
        }

        private string GenerateId(string pluginName, string suffix = "")
        {
            return _pluginsIdHelper.BuildId(pluginName, suffix);
        }

        private string GeneratePluginId(string pluginName)
        {
            var seed = 1;

            while (true)
            {
                var pluginId = GenerateId(pluginName, $"{seed}");
                if (!_plugins.ContainsKey(pluginId))
                    return pluginId;

                seed++;
            }
        }
    }
}
