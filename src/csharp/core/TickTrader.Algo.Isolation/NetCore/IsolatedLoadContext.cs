#if NETCOREAPP3_1_OR_GREATER
using System;
using System.Reflection;
using System.Runtime.Loader;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Package;

namespace TickTrader.Algo.Isolation
{
    internal sealed class IsolatedLoadContext : IPackageLoadContext
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<IsolatedLoadContext>();

        private readonly AssemblyLoadContext _childContext;
        private readonly LoadContextAdapter _adapter;


        public IsolatedLoadContext()
        {
            _childContext = new AssemblyLoadContext($"PackageLoadContext {Guid.NewGuid():N}", true);
            _adapter = new LoadContextAdapter(_childContext, false);
        }


        public void Dispose()
        {
            try
            {
                _adapter.Dispose();
                _childContext?.Unload();
            }
            catch (Exception ex)
            {
                _logger?.Error("Failed to unload child context: " + ex.Message);
            }
        }


        public PackageInfo Load(string pkgId, string pkgPath) => _adapter.Load(pkgId, pkgPath);

        public PackageInfo Load(string pkgId, byte[] pkgBinary) => _adapter.Load(pkgId, pkgBinary);

        public PackageInfo ScanAssembly(string pkgId, Assembly assembly) => throw new NotSupportedException("Assembly should be loaded into isolated context explicitly");
    }
}
#endif
