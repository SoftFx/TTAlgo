using System;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Package
{
    internal static class Errors
    {
        public static Exception DuplicateLocationId(string locationId) => new AlgoException($"Cannot register multiple paths for location '{locationId}'");

        public static Exception MissingPathForUploadLocation() => new AlgoException($"Path for upload locationId not found");

        public static Exception PackageNotFound(string pkgId) => new AlgoException($"Package '{pkgId}' not found");

        public static Exception PkgRefNotFound(string pkgRefId) => new AlgoException($"Package ref '{pkgRefId}' not found");

        public static Exception PackageLocked(string pkgId) => new AlgoException($"One or more trade bots from package '{pkgId}' is being executed! Please stop all bots and try again!");

        public static Exception MissingSynchronizationContext() => new AlgoException("SynchronizationContext is not found");

        public static Exception SynchronizationContextIsDifferent() => new AlgoException("Can't change object outside of original SynchronizationContext");
    }
}
