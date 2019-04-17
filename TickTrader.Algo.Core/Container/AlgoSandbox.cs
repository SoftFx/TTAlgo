using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Core.Container
{
    internal class AlgoSandbox : CrossDomainObject
    {
        private IPluginLoader loader;
        private List<Assembly> _loadedAssemblies = new List<Assembly>();

        public AlgoSandbox(IPluginLoader src, bool isolated)
        {
            this.loader = src;

            try
            {
                //CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
                //CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                src.Init();
                LoadAndInspect(src.MainAssemblyName);
                if (isolated)
                {
                    AlgoAssemblyInspector.FindReductions(Assembly.Load(ReductionCollection.EmbeddedReductionsAssemblyName)); // loading default reductions in plugin domain
                }
            }
            catch
            {
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
                throw;
            }
        }

        public IEnumerable<PluginMetadata> AlgoMetadata { get; private set; }

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
            AlgoMetadata = AlgoAssemblyInspector.FindPlugins(algoAssembly);
        }

        private Assembly LoadAssembly(string assemblyFileName)
        {
            string pdbFileName = Path.GetFileNameWithoutExtension(assemblyFileName) + ".pdb";

            byte[] assemblyBytes = loader.GetFileBytes(assemblyFileName);
            byte[] symbolsBytes = loader.GetFileBytes(pdbFileName);

            if (assemblyBytes == null)
                throw new FileNotFoundException($"Package {loader.MainAssemblyName} is missing required file '{assemblyFileName}'");

            var assembly = Assembly.Load(assemblyBytes, symbolsBytes);
            _loadedAssemblies.Add(assembly);
            return assembly;
        }

        public override void Dispose()
        {
            base.Dispose();

            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name);

            bool belongsToThisPackage = _loadedAssemblies.Contains(args.RequestingAssembly);

            if (!belongsToThisPackage)
                return null;

            // force plugins to use loaded Api
            if (name.Name == "TickTrader.Algo.Api")
                return typeof(TickTrader.Algo.Api.TradeBot).Assembly;

            try
            {
                return LoadAssembly(name.Name + ".dll");
            }
            catch (FileNotFoundException)
            {
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
