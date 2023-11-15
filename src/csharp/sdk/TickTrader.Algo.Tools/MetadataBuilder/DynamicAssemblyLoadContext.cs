#if NET6_0_OR_GREATER
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace TickTrader.Algo.Tools.MetadataBuilder
{
    internal sealed class DynamicAssemblyLoadContext : AssemblyLoadContext, IDisposable
    {
        private const string ApiFileName = "TickTrader.Algo.Api.dll";

        private readonly ConcurrentDictionary<string, Assembly> _cache = new ConcurrentDictionary<string, Assembly>();
        private readonly Action<string> _print;
        private readonly AssemblyDependencyResolver _resolver;


        internal string ApiVersion { get; private set; }


        internal DynamicAssemblyLoadContext(string mainDllPath, Action<string> print) : base(true)
        {
            _resolver = new AssemblyDependencyResolver(mainDllPath);
            _print = print;
        }

        public void Dispose()
        {
            _cache.Clear();
            Unload();
        }

        internal Assembly LoadAssemblyFileNotLocked(string path)
        {
            using (var file = File.OpenRead(path))
                return LoadFromStream(file);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            //_print($"\tResolving {assemblyName}");

            var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (!File.Exists(assemblyPath))
            {
                //_print($"\tAssembly {assemblyName} not found");
                return null;
            }

            _print($"\tLoading '{assemblyName}' from '{assemblyPath}'");

            var assembly = LoadAssemblyFileNotLocked(assemblyPath);
            if (Path.GetFileName(assemblyPath) == ApiFileName)
                // get version of assembly that was actually loaded
                ApiVersion = assembly.GetName().Version.ToString();

            return RegisterNewAssembly(assembly);
        }


        private Assembly RegisterNewAssembly(Assembly assembly)
        {
            var assemblyName = assembly.FullName;

            _print($"\tTry register new assembly {assemblyName}");

            _cache[assemblyName] = assembly;

            return assembly;
        }
    }
}
#endif
