using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core.Container
{
    internal class AlgoSandbox : CrossDomainObject
    {
        protected static readonly IAlgoCoreLogger _logger = CoreLoggerFactory.GetLogger("AlgoSandbox");
        private IPluginLoader loader;

        public AlgoSandbox(IPluginLoader src)
        {
            this.loader = src;
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                src.Init();
                LoadAndInspect(src.MainAssemblyName);
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

            byte[] assemblyBytes = loader.GetFileBytes(assemblyFileName);
            byte[] symbolsBytes = loader.GetFileBytes(pdbFileName);

            if (assemblyBytes == null)
                throw new FileNotFoundException($"Package {loader.MainAssemblyName} is missing required file '{assemblyFileName}'");

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

            try
            {
                return LoadAssembly(name.Name + ".dll");
            }
            catch (FileNotFoundException ex)
            {
                //_logger.Debug($"Failed to resolve assembly {name.Name}: {ex.Message}");
                return null;
            }
        }
    }

    internal interface IPluginLoader
    {
        string MainAssemblyName { get; }
        byte[] GetFileBytes(string packageLocalPath);
        void Init();
    }
}
