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
        public const char PackageIdSeparator = '/';
        public const string EmbeddedRepositoryId = "";
        public const string LocalRepositoryId = "local";
        public const string CommonRepositoryId = "common";


        public static string GetPackageExtensions => "Packages|*.ttalgo";

        public static string GetPackageAndAllExtensions => "Packages|*.ttalgo|All Files|*.*";

        public static bool IsFileSupported(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            return ext == ".ttalgo" || ext == ".dll";
        }

        public static string GetPackageIdFromPath(string locationId, string path)
        {
            var name = Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix
                ? Path.GetFileName(path) : Path.GetFileName(path).ToLowerInvariant();
            return GetPackageIdFromName(locationId, name);
        }

        public static string GetPackageIdFromName(string locationId, string name)
        {
            return string.IsNullOrEmpty(locationId) ? name : $"{locationId}{PackageIdSeparator}{name}";
        }

        public static void UnpackPackageId(string packageId, out string locationId, out string packageName)
        {
            var parts = packageId.Split(PackageIdSeparator);

            if (parts.Length > 2)
                throw new ArgumentException("Invalid package id");

            locationId = string.Empty;
            if (parts.Length == 1)
            {
                packageName = parts[0];
                return;
            }

            locationId = parts[0];
            packageName = parts[1];

        }

        public static PackageInfo GetInfo(string packageId, PackageIdentity identity, IEnumerable<PluginMetadata> plugins)
        {
            var res = new PackageInfo
            {
                PackageId = packageId,
                Identity = identity,

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
