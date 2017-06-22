using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    internal class PluginIdProvider
    {
        public const int MaxIdLength = 30;


        private Dictionary<string, int> _bots;


        public PluginIdProvider()
        {
            _bots = new Dictionary<string, int>();
        }


        public string GeneratePluginId(AlgoPluginDescriptor descriptor)
        {
            switch (descriptor.AlgoLogicType)
            {
                case AlgoTypes.Indicator:
                    return GenerateIndicatorId(descriptor.DisplayName);
                case AlgoTypes.Robot:
                    return GenerateBotId(descriptor.DisplayName);
                default:
                    return GenerateId(descriptor.DisplayName);
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
            if (pluginName.Length + suffix.Length > MaxIdLength)
            {
                pluginName = pluginName.Substring(0, MaxIdLength - suffix.Length);
            }
            return $"{pluginName}{suffix}";
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
                var botId = GenerateId(botName, $" {seed}");
                if (!_bots.ContainsKey(botId))
                    return botId;

                seed++;
            }
        }
    }
}
