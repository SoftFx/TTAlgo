using Machinarium.State;
using NLog;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Common.Model.Setup;

namespace TickTrader.BotTerminal
{
    internal class PluginModel : CrossDomainObject
    {
        private enum States { Strating, Running, Stopping }

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private StateMachine<States> stateControl = new StateMachine<States>();
        private PluginExecutor executor;
        private IAlgoPluginHost host;

        public PluginModel(PluginSetup pSetup, IAlgoPluginHost host)
        {
            this.host = host;
            this.Setup = pSetup;
            this.PluginRef = pSetup.PluginRef;
            this.Name = pSetup.Descriptor.DisplayName;

            executor = CreateExecutor();
            Setup.Apply(executor);
            
        }

        protected bool StartExcecutor()
        {
            try
            {
                host.UpdatePlugin(executor);
                executor.Start();
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "StartExcecutor() failed!");
                return false;
            }
        }

        protected Task StopExecutor()
        {
            return Task.Factory.StartNew(() => executor.Stop());
        }

        public AlgoPluginRef PluginRef { get; private set; }
        public PluginSetup Setup { get; private set; }
        public IAlgoPluginHost Host { get { return host; } }
        public string Name { get; set; }

        protected virtual PluginExecutor CreateExecutor()
        {
            var executor = PluginRef.CreateExecutor();
            executor.OnRuntimeError += Executor_OnRuntimeError;
            host.InitializePlugin(executor);
            return executor;
        }

        private void Executor_OnRuntimeError(Exception ex)
        {
            logger.Error(ex, "Exception in Algo executor! Name=" + Name);
        }
    }

    internal enum BotModelStates { Stopped, Running, Stopping }
}
