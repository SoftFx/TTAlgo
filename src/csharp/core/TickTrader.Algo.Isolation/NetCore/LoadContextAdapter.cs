#if NETCOREAPP3_1_OR_GREATER
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Package;

namespace TickTrader.Algo.Isolation
{
    internal sealed class LoadContextAdapter : IDisposable
    {
        private readonly object _lock = new object();
        private readonly List<IPackageLoader> _loaders = new List<IPackageLoader>();
        private readonly AssemblyLoadContext _context;
        private readonly bool _cachePkgMetadata;


        public LoadContextAdapter(AssemblyLoadContext context, bool cachePkgMetadata)
        {
            _context = context;
            _context.Resolving += OnAssemblyResolve;
            _cachePkgMetadata = cachePkgMetadata;
        }


        public void Dispose()
        {
            _context.Resolving -= OnAssemblyResolve;
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


        private PackageInfo LoadInternal(string pkgId, IPackageLoader loader)
        {
            lock (_lock)
            {
                _loaders.Add(loader);
            }
            var mainAssembly = LoadAssembly(loader, loader.MainAssemblyName);
            return PackageExplorer.ScanAssembly(pkgId, mainAssembly, _cachePkgMetadata);
        }

        private Assembly LoadAssembly(IPackageLoader loader, string assemblyFileName)
        {
            string pdbFileName = Path.GetFileNameWithoutExtension(assemblyFileName) + ".pdb";

            byte[] assemblyBytes = loader.GetFileBytes(assemblyFileName);
            byte[] symbolsBytes = loader.GetFileBytes(pdbFileName);

            if (assemblyBytes == null)
                return null;
            var assemblyStream = new MemoryStream(assemblyBytes);
            MemoryStream symbolsStream = symbolsBytes == null ? null : new MemoryStream(symbolsBytes);

            return _context.LoadFromStream(assemblyStream, symbolsStream);
        }


        private Assembly OnAssemblyResolve(AssemblyLoadContext ctx, AssemblyName name)
        {
            if (name.Name == "TickTrader.Algo.Api")
                return typeof(Api.AlgoPlugin).Assembly;

            var loadersSnapshot = new IPackageLoader[0];
            lock (_lock)
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
