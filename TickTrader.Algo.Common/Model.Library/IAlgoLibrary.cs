using System;
using System.Collections.Generic;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Package;

namespace TickTrader.Algo.Common.Model
{
    public interface IAlgoLibrary
    {
        event Action<UpdateInfo<PackageInfo>> PackageUpdated;

        event Action<UpdateInfo<PluginInfo>> PluginUpdated;

        event Action Reset;

        event Action<PackageInfo> PackageStateChanged;


        IEnumerable<PackageInfo> GetPackages();

        PackageInfo GetPackage(string packageId);

        IEnumerable<PluginInfo> GetPlugins();

        IEnumerable<PluginInfo> GetPlugins(Metadata.Types.PluginType type);

        PluginInfo GetPlugin(PluginKey key);

        AlgoPackageRef GetPackageRef(string packageId);
    }
}
