using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core.Repository
{
    internal class AlgoSandbox : CrossDomainObject
    {
        private IDotNetPluginPackage package;

        public AlgoSandbox(IDotNetPluginPackage package)
        {
            this.package = package;
            //this.dllFolder = Path.GetDirectoryName(filePath);
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                LoadAndInspect(package.MainAssemblyName);
            }
            catch
            {
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
                throw;
            }
        }

        public IEnumerable<AlgoPluginDescriptor> AlgoMetadata { get; private set; }

        internal T Activate<T>() where T : MarshalByRefObject, new()
        {
            return new T();
        }

        public PluginExecutor CreateExecutor(string pluginId)
        {
            return new PluginExecutor(pluginId);
        }

        private void LoadAndInspect(string filePath)
        {
            Assembly algoAssembly = LoadAssembly(filePath);
            AlgoMetadata = AlgoPluginDescriptor.InspectAssembly(algoAssembly);
        }

        private Assembly LoadAssembly(string assemblyFileName)
        {
            string pdbFileName = Path.GetFileNameWithoutExtension(assemblyFileName) + ".pdb";

            byte[] assemblyBytes = package.GetFileBytes(assemblyFileName);
            byte[] symbolsBytes = package.GetFileBytes(pdbFileName);

            if (assemblyBytes == null)
                throw new FileNotFoundException("Package is missing required file: " + assemblyFileName);

            return Assembly.Load(assemblyBytes, symbolsBytes);
        }

        public override void Dispose()
        {
            base.Dispose();

            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name);

            // force plugins to use loaded Api
            if (name.Name == "TickTrader.Algo.Api")
                return typeof(TickTrader.Algo.Api.TradeBot).Assembly;

            return LoadAssembly(name.Name + ".dll");
        }
    }

    internal interface IDotNetPluginPackage
    {
        string MainAssemblyName { get; }
        byte[] GetFileBytes(string packageLocalPath);
    }
}
