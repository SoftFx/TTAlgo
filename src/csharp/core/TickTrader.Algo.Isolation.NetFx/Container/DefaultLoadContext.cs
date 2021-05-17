using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Package;

namespace TickTrader.Algo.Isolation.NetFx
{
    internal sealed class DefaultLoadContext : IPackageLoadContext
    {
        private readonly object _lock = new object();
        private readonly List<IPackageLoader> _loaders;
        private readonly List<Assembly> _loadedAssemblies;


        public DefaultLoadContext()
        {
            _loaders = new List<IPackageLoader>();
            _loadedAssemblies = new List<Assembly>();

            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }


        public void Dispose()
        {
            throw new NotSupportedException("Can't unload current load context");
        }

        public Task<PackageInfo> ExamineAssembly(string packageId, Assembly assembly)
        {
            return Task.Factory.StartNew(() => PackageExplorer.ExamineAssembly(packageId, assembly));
        }

        public Task<PackageInfo> Load(string packageId, string packagePath)
        {
            return Task.Factory.StartNew(() => LoadInternal(packageId, packagePath));
        }


        internal PackageInfo LoadInternal(string packageId, string packagePath)
        {
            var loader = PackageLoader.CreateForPath(packagePath);
            loader.Init();
            lock (_lock)
            {
                _loaders.Add(loader);
            }
            var mainAssembly = LoadAssembly(loader, loader.MainAssemblyName);
            return PackageExplorer.ExamineAssembly(packageId, mainAssembly);
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
