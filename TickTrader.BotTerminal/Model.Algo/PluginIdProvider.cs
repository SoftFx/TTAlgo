using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    internal class PluginIdProvider : IPluginIdProvider
    {
        private BotIdHelper _botIdHelper;
        private Dictionary<string, int> _bots;


        public PluginIdProvider()
        {
            _bots = new Dictionary<string, int>();
            _botIdHelper = new BotIdHelper();
        }


        public string GeneratePluginId(PluginMetadataInfo descriptor)
        {
            switch (descriptor.Type)
            {
                case AlgoTypes.Indicator:
                    return GenerateIndicatorId(descriptor.DisplayName);
                case AlgoTypes.Robot:
                    return GenerateBotId(descriptor.DisplayName);
                default:
                    return GenerateId(descriptor.DisplayName);
            }
        }

        public bool IsValidPluginId(PluginMetadataInfo descriptor, string pluginId)
        {
            if (!_botIdHelper.Validate(pluginId))
            {
                return false;
            }

            switch (descriptor.Type)
            {
                case AlgoTypes.Indicator:
                    return true;
                case AlgoTypes.Robot:
                    return !_bots.ContainsKey(pluginId);
                default:
                    return true;
            }
        }

        public void AddPlugin(PluginModel plugin)
        {
            if (plugin.Setup.Metadata.Type == AlgoTypes.Robot)
            {
                _bots.Add(plugin.InstanceId, 1);
            }
        }

        public void RemovePlugin(string pluginId)
        {
            if (_bots.ContainsKey(pluginId))
            {
                _bots.Remove(pluginId);
            }
        }


        private string GenerateId(string pluginName, string suffix = "")
        {
            return _botIdHelper.BuildId(pluginName, suffix);
        }

        private string GenerateIndicatorId(string indicatorName)
        {
            return GenerateId(indicatorName);
        }

        private string GenerateBotId(string botName)
        {
            var seed = 1;

            while (true)
            {
                var botId = GenerateId(botName, $"{seed}");
                if (!_bots.ContainsKey(botId))
                    return botId;

                seed++;
            }
        }
    }
}
