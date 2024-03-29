﻿using System;
using System.Runtime.Serialization;

namespace TickTrader.Algo.Core.Config
{
    public enum RepositoryLocation
    {
        Embedded = 0,
        LocalRepository = 1,
        LocalExtensions = 2,
        CommonRepository = 3,
        CommonExtensions = 4,
    }


    [DataContract(Name = "PluginKey", Namespace = "TTAlgo.Config.v2")]
    public class PluginKey : IComparable<PluginKey>
    {
        [DataMember]
        public string PackageName { get; set; }

        [DataMember]
        public string PackageLocation { get; set; }

        [DataMember]
        public string DescriptorId { get; set; }


        public PluginKey()
        {
        }

        public PluginKey(PackageKey packageKey, string descriptorId)
            : this(packageKey.Name, packageKey.Location, descriptorId)
        {
        }

        public PluginKey(string packageName, string packageLocation, string descriptorId)
        {
            PackageName = packageName;
            PackageLocation = packageLocation;
            DescriptorId = descriptorId;
        }


        public override string ToString()
        {
            return $"Plugin {DescriptorId} in {PackageName} from {PackageLocation}";
        }

        public override int GetHashCode()
        {
            return $"{PackageName}{PackageLocation}{DescriptorId}".GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var key = obj as PluginKey;
            return key != null
                && key.DescriptorId == DescriptorId
                && key.PackageName == PackageName
                && key.PackageLocation == PackageLocation;
        }

        public bool IsFromPackage(PackageKey key)
        {
            return key.Name == PackageName && key.Location == PackageLocation;
        }

        public PackageKey GetPackageKey()
        {
            return new PackageKey(PackageName, PackageLocation);
        }

        public int CompareTo(PluginKey other)
        {
            var res1 = PackageName.CompareTo(other.PackageName);
            if (res1 == 0)
            {
                var res2 = PackageLocation.CompareTo(other.PackageLocation);
                if (res2 == 0)
                    return DescriptorId.CompareTo(DescriptorId);
                return res2;
            }
            return res1;
        }

        public PluginKey Clone()
        {
            return new PluginKey
            {
                PackageName = PackageName,
                PackageLocation = PackageLocation,
                DescriptorId = DescriptorId,
            };
        }
    }
}
