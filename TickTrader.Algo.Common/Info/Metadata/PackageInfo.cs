using System;
using System.Collections.Generic;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Common.Info
{
    public class PackageInfo
    {
        public PackageKey Key { get; set; }

        public PackageIdentity Identity { get; set; }

        public bool IsValid { get; set; }

        public List<PluginInfo> Plugins { get; set; }

        public bool IsLocked { get; set; }

        public bool IsObsolete { get; set; }


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
