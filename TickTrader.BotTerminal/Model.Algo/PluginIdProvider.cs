using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    internal class PluginIdProvider
    {
        public const int MaxIdLength = 30;
        public const string AllowedCharacters = "a-zA-Z0-9";


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
                    return GenerateIndicatorId(descriptor.UserDisplayName);
                case AlgoTypes.Robot:
                    return GenerateBotId(descriptor.UserDisplayName);
                default:
                    return GenerateId(descriptor.UserDisplayName);
            }
        }

        public bool IsValidPluginId(AlgoPluginDescriptor descriptor, string pluginId)
        {
            var match = Regex.Match(pluginId, $"^[{AllowedCharacters}]{{1,{MaxIdLength}}}$");
            if (!match.Success)
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
            var matches = Regex.Matches(pluginName, $"[{AllowedCharacters}]*");
            var pluginIdBuilder = new StringBuilder();
            foreach (Match match in matches)
            {
                pluginIdBuilder.Append(match.Value);
            }
            pluginName = pluginIdBuilder.ToString(0, Math.Min(MaxIdLength - suffix.Length, pluginIdBuilder.Length));
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
                var botId = GenerateId(botName, $"{seed}");
                if (!_bots.ContainsKey(botId))
                    return botId;

                seed++;
            }
        }
    }
}
