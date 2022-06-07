#if NETCOREAPP3_1_OR_GREATER
using System;
using System.Reflection;
using System.Runtime.Loader;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Package;

namespace TickTrader.Algo.Isolation
{
    internal sealed class DefaultLoadContext : IPackageLoadContext
    {
        private readonly LoadContextAdapter _adapter;


        public DefaultLoadContext()
        {
            _adapter = new LoadContextAdapter(AssemblyLoadContext.Default, true);
        }


        public void Dispose()
        {
            throw new NotSupportedException("Can't unload current load context");
        }

        public PackageInfo Load(string pkgId, string pkgPath) => _adapter.Load(pkgId, pkgPath);

        public PackageInfo Load(string pkgId, byte[] pkgBinary) => _adapter.Load(pkgId, pkgBinary);

        public PackageInfo ScanAssembly(string pkgId, Assembly assembly) => PackageExplorer.ScanAssembly(pkgId, assembly);
    }
}
#endif
