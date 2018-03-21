using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotAgent.BA.Entities
{
    public class PackageInfo
    {
        public PackageInfo(string name, DateTime created, bool isValid, IEnumerable<PluginInfo> plugins)
        {
            Name = name;
            Created = created;
            IsValid = isValid;
            Plugins = plugins.ToList();
        }

        public string Name { get; }
        public DateTime Created { get; }
        public bool IsValid { get; }

        public IReadOnlyList<PluginInfo> Plugins { get; }

        public IEnumerable<PluginInfo> GetPluginsByType(AlgoTypes robot)
        {
            return Plugins.Where(p => p.Descriptor.AlgoLogicType == AlgoTypes.Robot);
        }
    }
}
