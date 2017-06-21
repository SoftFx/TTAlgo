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

        private StateMachine<States> _stateControl = new StateMachine<States>();
        private PluginExecutor _executor;
        private IAlgoPluginHost _host;

        public AlgoPluginRef PluginRef { get; }

        public PluginSetup Setup { get; }

        public string InstanceId { get; }

        public bool Isolated { get; }

        public IAlgoPluginHost Host => _host;

        public PluginModel(PluginSetupViewModel pSetup, IAlgoPluginHost host)
        {
            _host = host;
            Setup = pSetup.Setup;
            PluginRef = Setup.PluginRef;
            InstanceId = pSetup.InstanceId;
            Isolated = pSetup.Isolated;

            _executor = CreateExecutor();
            Setup.SetWorkingFolder(_executor.WorkingFolder);
            Setup.Apply(_executor);
            
        }

        protected bool StartExcecutor()
        {
            try
            {
                _host.UpdatePlugin(_executor);
                _executor.Start();
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
            return Task.Factory.StartNew(() => _executor.Stop());
        }

        protected virtual PluginExecutor CreateExecutor()
        {
            var executor = PluginRef.CreateExecutor();

            executor.OnRuntimeError += Executor_OnRuntimeError;

            executor.InstanceId = InstanceId;
            executor.Isolated = Isolated;
            executor.WorkingFolder = EnvService.Instance.AlgoWorkingFolder;
            executor.BotWorkingFolder = EnvService.Instance.AlgoWorkingFolder;

            _host.InitializePlugin(executor);

            return executor;
        }

        private void Executor_OnRuntimeError(Exception ex)
        {
            logger.Error(ex, "Exception in Algo executor! InstanceId=" + InstanceId);
        }
    }

    internal enum BotModelStates { Stopped, Running, Stopping }
}
