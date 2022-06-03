#if NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Package;

namespace TickTrader.Algo.Isolation
{
    internal sealed class DefaultLoadContext : IPackageLoadContext
    {
        private readonly object _lock = new object();
        private readonly List<IPackageLoader> _loaders = new List<IPackageLoader>();
        private readonly List<Assembly> _loadedAssemblies = new List<Assembly>();
        private readonly bool _cacheScanResult;


        public DefaultLoadContext() : this(true) { }


        internal DefaultLoadContext(bool cacheScanResult)
        {
            _cacheScanResult = cacheScanResult;

            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }


        public void Dispose()
        {
            throw new NotSupportedException("Can't unload current load context");
        }

        public PackageInfo Load(string pkgId, string pkgPath)
        {
            var loader = PackageLoader.CreateForPath(pkgPath);
            loader.Init();
            return LoadInternal(pkgId, loader);
        }

        public PackageInfo Load(string pkgId, byte[] pkgBinary)
        {
            var loader = new ZipBinaryV1Loader(pkgBinary);
            loader.Init();
            return LoadInternal(pkgId, loader);
        }

        public PackageInfo ScanAssembly(string pkgId, Assembly assembly)
        {
            return PackageExplorer.ScanAssembly(pkgId, assembly, _cacheScanResult);
        }


        internal PackageInfo LoadInternal(string pkgId, IPackageLoader loader)
        {
            lock (_lock)
            {
                _loaders.Add(loader);
            }
            var mainAssembly = LoadAssembly(loader, loader.MainAssemblyName);
            return PackageExplorer.ScanAssembly(pkgId, mainAssembly, _cacheScanResult);
        }

        private Assembly LoadAssembly(IPackageLoader loader, string assemblyFileName)
        {
            string pdbFileName = Path.GetFileNameWithoutExtension(assemblyFileName) + ".pdb";

            byte[] assemblyBytes = loader.GetFileBytes(assemblyFileName);
            byte[] symbolsBytes = loader.GetFileBytes(pdbFileName);

            if (assemblyBytes == null)
                return null;

            var assembly = Assembly.Load(assemblyBytes, symbolsBytes);
            lock (_lock)
            {
                _loadedAssemblies.Add(assembly);
            }
            return assembly;
        }

        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name);

            lock (_lock)
            {
                if (!_loadedAssemblies.Contains(args.RequestingAssembly))
                    return null; // we don't need to resolve dependencies of assemblies that are not tracked
            }

            if (name.Name == "TickTrader.Algo.Api")
                return typeof(Api.AlgoPlugin).Assembly;

            var loadersSnapshot = new IPackageLoader[0];
            lock(_lock)
            {
                loadersSnapshot = _loaders.ToArray();
            }

            foreach (var loader in loadersSnapshot)
            {
                var assembly = LoadAssembly(loader, $"{name.Name}.dll");
                if (assembly != null)
                    return assembly;
            }

            return null;
        }
    }
}
#endif
