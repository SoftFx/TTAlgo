using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TickTrader.Algo.Core.Container;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Core
{
    public class PluginContainer : IDisposable
    {
        private IAlgoCoreLogger _logger;
        private Isolated<ChildDomainProxy> _subDomain;

        public IEnumerable<AlgoPluginRef> Plugins { get; protected set; }

        internal PluginContainer(IPluginLoader loader, IAlgoCoreLogger logger = null)
        {
            this._logger = logger;

            try
            {
                _subDomain = new Isolated<ChildDomainProxy>();
                var sandbox = _subDomain.Value.CreateDotNetSanbox(loader);
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
                if (_subDomain != null)
                    _subDomain.Dispose();
            }
            catch (Exception ex)
            {
                _logger?.Debug("Failed to unload child domain: " + ex.Message);
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
