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

        public static string GetPackageId(RepositoryLocation location, string path)
        {
            var name = Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix
                ? Path.GetFileName(path) : Path.GetFileName(path).ToLowerInvariant();
            return $"{location}/{name}";
        }

        public static string GetPackageId(PackageKey packageKey)
        {
            return $"{packageKey.Location}/{packageKey.Name}";
        }

        public static PackageKey GetPackageKey(RepositoryLocation location, string path)
        {
            var name = Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix
                ? Path.GetFileName(path) : Path.GetFileName(path).ToLowerInvariant();
            return new PackageKey(name, location);
        }

        public static PackageInfo GetInfo(PackageKey key, PackageIdentity identity, IEnumerable<PluginMetadata> plugins)
        {
            var res = new PackageInfo
            {
                Key = key,
                Identity = identity,

            };
            res.Plugins.AddRange(plugins.Select(p => GetInfo(res.Key, p)));
            return res;
        }

        public static PluginInfo GetInfo(PackageKey packageId, PluginMetadata plugin)
        {
            return new PluginInfo
            {
                Key = new PluginKey(packageId, plugin.Id),
                Descriptor_ = plugin.Descriptor,
            };
        }
    }
}
