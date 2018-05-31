using System;
using System.Collections.Generic;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Common.Model
{
    public interface IAlgoLibrary
    {
        event Action<UpdateInfo<PackageInfo>> PackageUpdated;

        event Action<UpdateInfo<PluginInfo>> PluginUpdated;

        event Action Reset;


        IEnumerable<PackageInfo> GetPackages();

        PackageInfo GetPackage(PackageKey key);

        IEnumerable<PluginInfo> GetPlugins();

        IEnumerable<PluginInfo> GetPlugins(AlgoTypes type);

        PluginInfo GetPlugin(PluginKey key);

        AlgoPackageRef GetPackageRef(PackageKey key);

        AlgoPluginRef GetPluginRef(PluginKey key);
    }
}
