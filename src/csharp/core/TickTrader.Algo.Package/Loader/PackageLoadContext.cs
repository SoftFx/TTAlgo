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
        /// Load package assemblies for exeution
        /// </summary>
        Task<PackageInfo> Load(string packageId, string packagePath);

        /// <summary>
        /// Generate metadata for already loaded assemblies
        /// </summary>
        Task<PackageInfo> ExamineAssembly(string packageId, Assembly assembly);
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
        public static Task<PackageInfo> Load(string packageId, string packagePath)
        {
            return _default.Load(packageId, packagePath);
        }

        /// <summary>
        /// Generates metadata of a dependent assembly already loaded other ways
        /// </summary>
        public static Task<PackageInfo> ExamineAssembly(string packageId, Assembly assembly)
        {
            return _default.ExamineAssembly(packageId, assembly);
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
                    res = await loadContext.Load("reflection-only", packagePath);
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
