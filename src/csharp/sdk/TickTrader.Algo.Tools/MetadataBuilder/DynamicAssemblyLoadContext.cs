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
        private readonly string _source;


        internal string ApiVersion { get; private set; }


        internal DynamicAssemblyLoadContext(string sorceFolder, Action<string> print)
        {
            _source = sorceFolder;
            _print = print;

            Resolving += DynamicResolving;
        }

        public void Dispose() => Resolving -= DynamicResolving;


        protected override Assembly Load(AssemblyName assemblyName) => null; //must return null


        private Assembly DynamicResolving(AssemblyLoadContext context, AssemblyName assemblyName)
        {
            _print($"\tResolving {assemblyName}");

            if (_cache.TryGetValue(assemblyName.FullName, out var assembly))
                return assembly;

            var expectedDllName = Path.Combine(Environment.CurrentDirectory, _source, $"{assemblyName.Name}.dll");

            if (File.Exists(expectedDllName))
            {
                var dllAssemblyName = GetAssemblyName(expectedDllName);
                var dllFullName = dllAssemblyName.FullName;

                if (assemblyName.FullName == dllFullName)
                {
                    _print($"\tLoading {dllFullName}");

                    if (Path.GetFileName(expectedDllName) == ApiFileName)
                        ApiVersion = dllAssemblyName.Version.ToString();

                    return RegisterNewAssembly(context.LoadFromAssemblyPath(expectedDllName));
                }
            }

            _print($"\tAssembly {assemblyName} not found");

            return null;
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
