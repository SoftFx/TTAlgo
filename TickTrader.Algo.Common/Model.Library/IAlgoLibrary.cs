using System;
using System.Collections.Generic;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model
{
    public interface IAlgoLibrary
    {
        event Action<PackageInfo> PackageAdded;
        event Action<PackageInfo> PackageReplaced;
        event Action<PackageInfo> PackageRemoved;

        event Action<PluginInfo> PluginAdded;
        event Action<PluginInfo> PluginReplaced;
        event Action<PluginInfo> PluginRemoved;


        IEnumerable<PackageInfo> GetPackages();

        PackageInfo GetPackage(PackageKey key);

        IEnumerable<PluginInfo> GetPlugins();

        IEnumerable<PluginInfo> GetPlugins(AlgoTypes type);

        PluginInfo GetPlugin(PluginKey key);

        AlgoPackageRef GetPackageRef(PackageKey key);

        AlgoPluginRef GetPluginRef(PluginKey key);
    }
}
