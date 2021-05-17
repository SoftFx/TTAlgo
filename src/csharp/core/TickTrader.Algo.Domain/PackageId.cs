using System;
using System.IO;

namespace TickTrader.Algo.Domain
{
    public class PackageId : IEquatable<PackageId>, IComparable<PackageId>
    {
        public string PackedStr { get; }

        public string LocationId { get; }

        public string PackageName { get; }


        public PackageId(string locationId, string pkgName)
            : this(Pack(locationId, pkgName), locationId, pkgName)
        {
        }

        private PackageId(string packedStr, string locationId, string pkgName)
        {
            PackedStr = packedStr;
            LocationId = locationId;
            PackageName = pkgName;
        }


        public static string Pack(string locationId, string pkgName)
        {
            if (string.IsNullOrWhiteSpace(locationId))
                throw new ArgumentException($"'{nameof(locationId)}' can't be empty string");
            if (string.IsNullOrWhiteSpace(pkgName))
                throw new ArgumentException($"'{nameof(pkgName)}' can't be empty string");

            return string.Join(SharedConstants.IdSeparatorStr, SharedConstants.PackageIdPrefix, locationId, pkgName.ToLowerInvariant());
        }

        public static string FromPath(string locationId, string pkgPath)
        {
            return Pack(locationId, Path.GetFileName(pkgPath));
        }

        public static bool TryUnpack(string packedId, out PackageId pkgId)
        {
            pkgId = new PackageId(packedId, SharedConstants.InvalidIdPart, SharedConstants.InvalidIdPart);

            var parts = packedId.Split(SharedConstants.IdSeparator);

            if (parts.Length != 3 || parts[0] != SharedConstants.PackageIdPrefix)
                return false;

            pkgId = new PackageId(packedId, parts[1], parts[2]);

            return true;
        }

        public static void Unpack(string packedId, out PackageId pkgId)
        {
            if (!TryUnpack(packedId, out pkgId))
                throw new ArgumentException("Invalid package id");
        }


        public override int GetHashCode()
        {
            return PackedStr.GetHashCode();
        }

        public override bool Equals(object other)
        {
            return Equals(other as PackageId);
        }

        public bool Equals(PackageId other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return PackedStr == other.PackedStr;
        }

        public int CompareTo(PackageId other)
        {
            if (other == null)
                return 1;

            if (ReferenceEquals(this, other))
                return 0;

            return StringComparer.Ordinal.Compare(PackedStr, other.PackedStr);
        }
    }
}
