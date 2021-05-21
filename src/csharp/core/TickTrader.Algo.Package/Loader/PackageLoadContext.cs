using System;
using System.Reflection;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Package
{
    public interface IPackageLoadContext : IDisposable
    {
        /// <summary>
        /// Load package assemblies for execution
        /// </summary>
        PackageInfo Load(string packageId, string packagePath);

        /// <summary>
        /// Load package assemblies for execution
        /// </summary>
        Task<PackageInfo> LoadAsync(string packageId, string packagePath);

        /// <summary>
        /// Generate metadata for already loaded assemblies
        /// </summary>
        PackageInfo ScanAssembly(string packageId, Assembly assembly);
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
        public static PackageInfo Load(string packageId, string packagePath)
        {
            return _default.Load(packageId, packagePath);
        }

        /// <summary>
        /// Load package into default load context. Loaded assemblies cannot be removed from process
        /// </summary>
        public static Task<PackageInfo> LoadAsync(string packageId, string packagePath)
        {
            return _default.LoadAsync(packageId, packagePath);
        }

        /// <summary>
        /// Generates metadata of a dependent assembly already loaded other ways
        /// </summary>
        public static PackageInfo ScanAssembly(string packageId, Assembly assembly)
        {
            return _default.ScanAssembly(packageId, assembly);
        }

        /// <summary>
        /// Loads package into isolated load context to extract PackageInfo. Isolated context is disposed after that
        /// </summary>
        public static PackageInfo ReflectionOnlyLoad(string packageId, string packagePath)
        {
            PackageInfo res = null;
            try
            {
                using (var loadContext = _factory(true))
                {
                    res = loadContext.Load(packageId, packagePath);
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
