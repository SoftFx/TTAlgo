using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Repository
{
    public static class PackageHelper
    {
        public static string GetPackageExtensions => "Packages|*.ttalgo";

        public static string GetPackageAndAllExtensions => "Packages|*.ttalgo|All Files|*.*";

        public static bool IsFileSupported(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            return ext == ".ttalgo" || ext == ".dll";
        }

        public static string GetPackageIdFromPath(string locationId, string path)
        {
            return PackageId.Pack(locationId, Path.GetFileName(path));
        }

        public static PackageInfo GetInfo(string packageId, PackageIdentity identity, IEnumerable<PluginMetadata> plugins)
        {
            var res = new PackageInfo
            {
                PackageId = packageId,
                Identity = identity,
                IsValid = true,
            };
            res.Plugins.AddRange(plugins.Select(p => GetInfo(res.PackageId, p)));
            return res;
        }

        public static PluginInfo GetInfo(string packageId, PluginMetadata plugin)
        {
            return new PluginInfo
            {
                Key = new PluginKey(packageId, plugin.Id),
                Descriptor_ = plugin.Descriptor,
            };
        }
    }
}
