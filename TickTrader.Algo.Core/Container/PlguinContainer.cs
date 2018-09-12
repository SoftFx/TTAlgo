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
        protected IAlgoCoreLogger Logger { get; }

        public IEnumerable<AlgoPluginRef> Plugins { get; protected set; }

        private PluginContainer(IAlgoCoreLogger logger = null)
        {
            Logger = logger;
        }

        internal virtual void DoLoad(IPluginLoader loader)
        {
            var sandbox =  new AlgoSandbox(loader, false);
            Plugins = sandbox.AlgoMetadata.Select(d => new AlgoPluginRef(d));
        }

        public static PluginContainer Load(string filePath, bool isolate, IAlgoCoreLogger logger = null)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();

            IPluginLoader loader;
            PluginContainer container;

            if (ext == ".ttalgo")
                loader = new PackageLoader(filePath);
            else if (ext == ".dll")
                loader = new DllLoader(filePath);
            else
                throw new ArgumentException("Unrecognized file type: " + ext);

            if (isolate)
                container = new IsolatedContainer(logger);
            else
                container = new PluginContainer(logger);

            container.DoLoad(loader);

            return container;
        }

        public virtual void Dispose() { }

        private class IsolatedContainer : PluginContainer
        {
            private Isolated<ChildDomainProxy> subDomain;

            public IsolatedContainer(IAlgoCoreLogger logger = null)
                : base(logger)
            {
            }

            internal override void DoLoad(IPluginLoader loader)
            {
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

            public override void Dispose()
            {
                try
                {
                    if (subDomain != null)
                        subDomain.Dispose();
                }
                catch (Exception ex)
                {
                    Logger?.Debug("Failed to unload child domain: " + ex.Message);
                }
            }
        }

        internal class ChildDomainProxy : CrossDomainObject
        {
            public AlgoSandbox CreateDotNetSanbox(IPluginLoader netPackage)
            {
                return new AlgoSandbox(netPackage, true);
            }
        }
    }
}
