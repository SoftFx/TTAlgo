using System;
using System.Collections.Generic;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Model
{
    public interface IAlgoLibrary
    {
        event Action<UpdateInfo<PackageInfo>> PackageUpdated;

        event Action<UpdateInfo<PluginInfo>> PluginUpdated;

        event Action Reset;

        event Action<PackageInfo> PackageStateChanged;


        IEnumerable<PackageInfo> GetPackages();

        PackageInfo GetPackage(PackageKey key);

        IEnumerable<PluginInfo> GetPlugins();

        IEnumerable<PluginInfo> GetPlugins(Metadata.Types.PluginType type);

        PluginInfo GetPlugin(PluginKey key);

        AlgoPackageRef GetPackageRef(PackageKey key);
    }
}
