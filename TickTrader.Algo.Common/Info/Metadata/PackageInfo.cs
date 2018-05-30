using System;
using System.Collections.Generic;

namespace TickTrader.Algo.Common.Info
{
    public class PackageInfo
    {
        public PackageKey Key { get; set; }

        public DateTime CreatedUtc { get; set; }

        public bool IsValid { get; set; }

        public List<PluginInfo> Plugins { get; set; }


        public PackageInfo()
        {
            Plugins = new List<PluginInfo>();
        }


        public override string ToString()
        {
            return Key.ToString();
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}
