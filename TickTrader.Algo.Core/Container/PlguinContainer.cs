using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Container;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core
{
    public class PluginContainer : IDisposable
    {
        private IAlgoCoreLogger logger;
        private Isolated<ChildDomainProxy> subDomain;

        public IEnumerable<AlgoPluginRef> Plugins { get; protected set; }

        internal PluginContainer(IPluginLoader loader, IAlgoCoreLogger logger = null)
        {
            this.logger = logger;

            try
            {
                subDomain = new Isolated<ChildDomainProxy>();
                var sandbox = subDomain.Value.CreateDotNetSanbox(loader);
                Plugins = sandbox.AlgoMetadata.Select(d => new IsolatedPluginRef(d, sandbox));
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }

        public static PluginContainer Load(string filePath, IAlgoCoreLogger logger = null)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();

            if (ext == ".ttalgo")
                return new PluginContainer(new PackageLoader(filePath), logger);
            else if (ext == ".dll")
                return new PluginContainer(new DllLoader(filePath), logger);
            else
                throw new ArgumentException("Unrecognized file type: " + ext);
        }

        public void Dispose()
        {
            try
            {
                if (subDomain != null)
                    subDomain.Dispose();
            }
            catch (Exception ex)
            {
                logger?.Debug("Failed to unload child domain: " + ex.Message);
            }
        }

        internal class ChildDomainProxy : CrossDomainObject
        {
            public AlgoSandbox CreateDotNetSanbox(IPluginLoader netPackage)
            {
                return new AlgoSandbox(netPackage);
            }
        }
    }
}
