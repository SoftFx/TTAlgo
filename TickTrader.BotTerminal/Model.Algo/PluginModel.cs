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

        public string PluginFilePath { get; }

        public PluginSetupViewModel Setup { get; private set; }

        public string InstanceId => Setup.InstanceId;

        public IAlgoPluginHost Host => _host;

        public PluginModel(SetupPluginViewModel pSetup, IAlgoPluginHost host)
        {
            _host = host;
            Setup = pSetup.Setup;
            PluginRef = Setup.PluginRef;
            PluginFilePath = pSetup.PluginItem.FilePath;

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

        protected void Configurate(PluginSetupViewModel setup)
        {
            Setup = setup;

            Setup.SetWorkingFolder(_executor.WorkingFolder);
            Setup.Apply(_executor);
            _executor.Permissions = Setup.Permissions;
        }

        protected Task StopExecutor()
        {
            return Task.Factory.StartNew(() => _executor.Stop());
        }

        protected void AbortExecutor()
        {
            _executor.Abort();
        }

        protected virtual PluginExecutor CreateExecutor()
        {
            var executor = PluginRef.CreateExecutor();

            executor.OnRuntimeError += Executor_OnRuntimeError;

            executor.TimeFrame = Setup.SelectedTimeFrame;
            executor.MainSymbolCode = Setup.MainSymbol.Name;
            executor.InstanceId = InstanceId;
            executor.Permissions = Setup.Permissions;
            executor.WorkingFolder = EnvService.Instance.AlgoWorkingFolder;
            executor.BotWorkingFolder = EnvService.Instance.AlgoWorkingFolder;

            _host.InitializePlugin(executor);

            return executor;
        }

        protected virtual void HandleReconnect()
        {
            _executor.HandleReconnect();
        }

        private void Executor_OnRuntimeError(Exception ex)
        {
            logger.Error(ex, "Exception in Algo executor! InstanceId=" + InstanceId);
        }
    }

    internal enum BotModelStates { Stopped, Running, Stopping }
}
