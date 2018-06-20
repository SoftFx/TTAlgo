using Machinarium.State;
using NLog;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Common.Model.Config;

namespace TickTrader.BotTerminal
{
    internal class PluginModel : CrossDomainObject
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private PluginExecutor _executor;


        public PluginConfig Config { get; private set; }

        public string InstanceId => Config.InstanceId;

        public AlgoPackageRef PackageRef { get; private set; }

        public AlgoPluginRef PluginRef { get; private set; }

        public PluginSetupModel Setup { get; private set; }

        public IAlgoPluginHost Host { get; }


        protected LocalAgent Agent { get; }

        protected IAlgoSetupContext SetupContext { get; }


        public PluginModel(PluginConfig config, LocalAgent agent, IAlgoPluginHost host, IAlgoSetupContext setupContext)
        {
            Config = config;
            Agent = agent;
            Host = host;
            SetupContext = setupContext;
        }

        protected bool StartExcecutor()
        {
            try
            {
                PackageRef = Agent.Library.GetPackageRef(Config.Key.GetPackageKey());
                PackageRef.IncrementRef();
                PluginRef = Agent.Library.GetPluginRef(Config.Key);
                Setup = Algo.Common.Model.Setup.AlgoSetupFactory.CreateSetup(PluginRef, Agent, SetupContext);
                Setup.Load(Config);

                _executor = CreateExecutor();
                Setup.SetWorkingFolder(_executor.WorkingFolder);
                Setup.Apply(_executor);

                Host.UpdatePlugin(_executor);
                _executor.Start();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "StartExcecutor() failed!");
                PackageRef?.DecrementRef();

                return false;
            }
        }

        internal virtual void Configurate(PluginConfig config)
        {
            Config = config;
        }

        protected Task StopExecutor()
        {
            return Task.Factory.StartNew(() =>
            {
                _executor.Stop();

                PackageRef?.DecrementRef();
                PackageRef = null;
                PluginRef = null;
            });
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
            executor.MainSymbolCode = Setup.MainSymbol;
            executor.InstanceId = InstanceId;
            executor.Permissions = Setup.Permissions;
            executor.WorkingFolder = EnvService.Instance.AlgoWorkingFolder;
            executor.BotWorkingFolder = EnvService.Instance.AlgoWorkingFolder;

            Host.InitializePlugin(executor);

            return executor;
        }

        protected virtual void HandleReconnect()
        {
            _executor.HandleReconnect();
        }

        private void Executor_OnRuntimeError(Exception ex)
        {
            _logger.Error(ex, "Exception in Algo executor! InstanceId=" + InstanceId);
        }
    }

    internal enum BotModelStates { Stopped, Running, Stopping }
}
