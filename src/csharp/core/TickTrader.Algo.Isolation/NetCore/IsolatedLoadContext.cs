#if NETCOREAPP3_1_OR_GREATER
using System;
using System.Collections.Generic;
using System.IO;
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

        private readonly object _lock = new object();
        private readonly List<IPackageLoader> _loaders = new List<IPackageLoader>();
        private readonly AssemblyLoadContext _childContext;


        public IsolatedLoadContext()
        {
            _childContext = new AssemblyLoadContext($"PackageLoadContext {Guid.NewGuid():N}", true);
            _childContext.Resolving += OnAssemblyResolve;
        }


        public void Dispose()
        {
            try
            {
                _childContext?.Unload();
            }
            catch (Exception ex)
            {
                _logger?.Error("Failed to unload child context: " + ex.Message);
            }
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

        public PackageInfo ScanAssembly(string pkgId, Assembly assembly) => throw new NotSupportedException("Assembly should be loaded into isolated context explicitly");


        internal PackageInfo LoadInternal(string pkgId, IPackageLoader loader)
        {
            lock (_lock)
            {
                _loaders.Add(loader);
            }
            var mainAssembly = LoadAssembly(loader, loader.MainAssemblyName);
            return PackageExplorer.ScanAssembly(pkgId, mainAssembly, false);
        }

        private Assembly LoadAssembly(IPackageLoader loader, string assemblyFileName)
        {
            string pdbFileName = Path.GetFileNameWithoutExtension(assemblyFileName) + ".pdb";

            byte[] assemblyBytes = loader.GetFileBytes(assemblyFileName);
            byte[] symbolsBytes = loader.GetFileBytes(pdbFileName);

            if (assemblyBytes == null)
                return null;

            return _childContext.LoadFromStream(new MemoryStream(assemblyBytes), new MemoryStream(symbolsBytes));
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
