using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    internal class PluginIdProvider
    {
        private BotIdHelper _botIdHelper;
        private Dictionary<string, int> _bots;


        public PluginIdProvider()
        {
            _bots = new Dictionary<string, int>();
            _botIdHelper = new BotIdHelper();
        }


        public string GeneratePluginId(AlgoPluginDescriptor descriptor)
        {
            switch (descriptor.AlgoLogicType)
            {
                case AlgoTypes.Indicator:
                    return GenerateIndicatorId(descriptor.UserDisplayName);
                case AlgoTypes.Robot:
                    return GenerateBotId(descriptor.UserDisplayName);
                default:
                    return GenerateId(descriptor.UserDisplayName);
            }
        }

        public bool IsValidPluginId(AlgoPluginDescriptor descriptor, string pluginId)
        {
            if (!_botIdHelper.Validate(pluginId))
            {
                return false;
            }

            switch (descriptor.AlgoLogicType)
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
            if (plugin.Setup.Descriptor.AlgoLogicType == AlgoTypes.Robot)
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
