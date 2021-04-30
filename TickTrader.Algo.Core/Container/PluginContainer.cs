using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Core.Container;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Core
{
    internal class PluginContainer : IDisposable
    {
        protected AlgoSandbox _sandbox;


        protected IAlgoLogger Logger { get; }

        public IEnumerable<AlgoPluginRef> Plugins { get; protected set; }


        private PluginContainer(IAlgoLogger logger = null)
        {
            Logger = logger;
        }


        public PluginExecutorCore CreateExecutor(string pluginId)
        {
            return _sandbox.CreateExecutor(pluginId);
        }

        public PluignExecutorFactory CreateExecutorFactory(string pluginId)
        {
            return _sandbox.CreateExecutorFactory(pluginId);
        }

        public T CreateObject<T>() where T : MarshalByRefObject, new()
        {
            return _sandbox.Activate<T>();
        }


        protected virtual void DoLoad(string filePath)
        {
            _sandbox = new AlgoSandbox(filePath, false);
            Plugins = _sandbox.AlgoMetadata.Select(d => new AlgoPluginRef(d, filePath));
        }

        protected virtual void CreateSandbox(IPluginLoader loader)
        {
        }


        public static PluginContainer Load(string filePath, bool isolate, IAlgoLogger logger = null)
        {
            PluginContainer container;

            if (isolate)
                container = new IsolatedContainer(logger);
            else
                container = new PluginContainer(logger);

            container.DoLoad(filePath);

            return container;
        }

        public virtual void Dispose() { }


        private class IsolatedContainer : PluginContainer
        {
            private class ChildDomainProxy : CrossDomainObject
            {
                public AlgoSandbox CreateDotNetSanbox(string packagePath)
                {
                    return new AlgoSandbox(packagePath, true);
                }
            }


            private Isolated<ChildDomainProxy> _subDomain;

            public IsolatedContainer(IAlgoLogger logger = null)
                : base(logger)
            {
            }

            protected override void DoLoad(string packagePath)
            {
                try
                {
                    _subDomain = new Isolated<ChildDomainProxy>();
                    _sandbox = _subDomain.Value.CreateDotNetSanbox(packagePath);
                    Plugins = _sandbox.AlgoMetadata.Select(d => new IsolatedPluginRef(d, packagePath));
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
                    _subDomain?.Dispose();
                }
                catch (Exception ex)
                {
                    Logger?.Debug("Failed to unload child domain: " + ex.Message);
                }
            }
        }
    }
}
