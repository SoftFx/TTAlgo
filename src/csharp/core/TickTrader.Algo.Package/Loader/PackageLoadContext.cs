using System;
using System.Reflection;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Package
{
    public interface IPackageLoadContext : IDisposable
    {
        /// <summary>
        /// Load package assemblies for execution
        /// </summary>
        PackageInfo Load(string pkgId, string pkgPath);

        /// <summary>
        /// Load package assemblies for execution
        /// </summary>
        PackageInfo Load(string pkgId, byte[] pkgBinary);

        /// <summary>
        /// Generate metadata for already loaded assemblies
        /// </summary>
        PackageInfo ScanAssembly(string pkgId, Assembly assembly);
    }

    public static class PackageLoadContext
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger(nameof(PackageLoadContext));

        private static Func<bool, IPackageLoadContext> _factory;
        private static IPackageLoadContext _default;


        public static void Init(Func<bool, IPackageLoadContext> factory)
        {
            if (_factory != null)
                throw new InvalidOperationException("Already initialized");

            _factory = factory;
            _default = factory(false); // Not isolated load context. Used for loading assemlies for execution. Loaded assemblies cannot be removed from process
        }


        /// <summary>
        /// Load package into default load context. Loaded assemblies cannot be removed from process
        /// </summary>
        public static PackageInfo Load(string pkgId, string pkgPath)
        {
            return _default.Load(pkgId, pkgPath);
        }

        /// <summary>
        /// Load package into default load context. Loaded assemblies cannot be removed from process
        /// </summary>
        public static PackageInfo Load(string pkgId, byte[] pkgBinary)
        {
            return _default.Load(pkgId, pkgBinary);
        }

        /// <summary>
        /// Generates metadata of a dependent assembly already loaded other ways
        /// </summary>
        public static PackageInfo ScanAssembly(string pkgId, Assembly assembly)
        {
            return _default.ScanAssembly(pkgId, assembly);
        }

        /// <summary>
        /// Loads package into isolated load context to extract PackageInfo. Isolated context is disposed after that
        /// </summary>
        public static PackageInfo ReflectionOnlyLoad(string pkgId, string pkgPath)
        {
            PackageInfo res = null;
            try
            {
                using (var loadContext = _factory(true))
                {
                    res = loadContext.Load(pkgId, pkgPath);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "RefletionOnlyLoad failed");
            }
            return res;
        }
    }
}
