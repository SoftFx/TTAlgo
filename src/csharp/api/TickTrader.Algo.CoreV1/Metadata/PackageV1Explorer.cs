using System.Linq;
using System.Reflection;
using TickTrader.Algo.CoreV1.Metadata;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Package;

namespace TickTrader.Algo.CoreV1
{
    public class PackageV1Explorer : IPackageExplorer
    {
        private PackageV1Explorer() { }


        public static IPackageExplorer Create() => new PackageV1Explorer();


        public PackageInfo ScanAssembly(string packageId, Assembly assembly)
        {
            var metadata = PackageMetadataCache.ExamineAssembly(packageId, assembly);
            var pkgInfo = new PackageInfo
            {
                PackageId = packageId,
            };
            pkgInfo.Plugins.AddRange(metadata.Plugins.Select(p => GetInfo(packageId, p)));
            pkgInfo.Reductions.AddRange(metadata.Reductions.Select(r => GetInfo(packageId, r)));
            return pkgInfo;
        }

        public static PluginInfo GetInfo(string packageId, PluginMetadata plugin)
        {
            return new PluginInfo
            {
                Key = new PluginKey(packageId, plugin.Id),
                Descriptor_ = plugin.Descriptor,
            };
        }

        public static ReductionInfo GetInfo(string packageId, ReductionMetadata reduction)
        {
            return new ReductionInfo
            {
                Key = new ReductionKey(packageId, reduction.Id),
                Descriptor_ = reduction.Descriptor,
            };
        }
    }
}
