using System;

namespace TickTrader.Algo.Domain
{
    public static class PackageId
    {
        public static string Pack(string locationId, string pkgName)
        {
            if (string.IsNullOrWhiteSpace(pkgName))
                throw new ArgumentException($"'{nameof(pkgName)}' can't be empty string");

            return string.IsNullOrWhiteSpace(locationId)
                ? string.Join(SharedConstants.IdSeparatorStr, SharedConstants.PackageIdPrefix, pkgName)
                : string.Join(SharedConstants.IdSeparatorStr, SharedConstants.PackageIdPrefix, locationId, pkgName);
        }

        public static bool TryUnpack(string pkgId, out string locationId, out string pkgName)
        {
            locationId = SharedConstants.InvalidIdPart;
            pkgName = SharedConstants.InvalidIdPart;

            var parts = pkgId.Split(SharedConstants.IdSeparator);

            if ((parts.Length != 2 && parts.Length != 3) || parts[0] != SharedConstants.PackageIdPrefix)
                return false;

            if (parts.Length == 2)
            {
                locationId = string.Empty;
                pkgName = parts[1];
            }
            else
            {
                locationId = parts[1];
                pkgName = parts[2];
            }

            return true;
        }

        public static void Unpack(string pkgId, out string locationId, out string pkgName)
        {
            if (!TryUnpack(pkgId, out locationId, out pkgName))
                throw new ArgumentException("Invalid package id");
        }
    }
}
