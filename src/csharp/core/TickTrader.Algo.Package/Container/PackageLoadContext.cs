using System;
using System.Reflection;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Package
{
    public interface IPackageLoadContext : IDisposable
    {
        // GetPackage
        // GetPlugin
        // GetReduction


        /// <summary>
        /// Load package assemblies for exeution
        /// </summary>
        Task<PackageInfo> Load(string packagePath);

        /// <summary>
        /// Generate metadata for already loaded assemblies
        /// </summary>
        Task<PackageInfo> InpectAssembly(Assembly assembly);
    }

    public static class PackageLoadContext
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger(nameof(PackageLoadContext));

        private static Func<bool, IPackageLoadContext> _factory;


        /// <summary>
        /// Not isolated load context. Used for loading assemlies for execution. Loaded assemblies cannot be removed from process
        /// </summary>
        public static IPackageLoadContext Default { get; private set; }


        public static void Init(Func<bool, IPackageLoadContext> factory)
        {
            if (_factory != null)
                throw new InvalidOperationException("Already initialized");

            _factory = factory;
            Default = factory(false);
        }


        /// <summary>
        /// Load package into default load context. Loaded assemblies cannot be removed from process
        /// </summary>
        public static Task<PackageInfo> Load(string packagePath)
        {
            return Default.Load(packagePath);
        }

        /// <summary>
        /// Generates metadata of a dependent assembly already loaded other ways
        /// </summary>
        public static Task<PackageInfo> InspectAssembly(Assembly assembly)
        {
            return Default.InpectAssembly(assembly);
        }

        /// <summary>
        /// Loads package into isolated load context to extract PackageInfo. Isolated context is disposed after that
        /// </summary>
        public static async Task<PackageInfo> ReflectionOnlyLoad(string packagePath)
        {
            PackageInfo res = null;
            try
            {
                using (var loadContext = _factory(true))
                {
                    res = await loadContext.Load(packagePath);
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
