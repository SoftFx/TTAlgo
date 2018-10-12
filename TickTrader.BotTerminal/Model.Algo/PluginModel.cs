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
using TickTrader.Algo.Common.Info;

namespace TickTrader.BotTerminal
{
    internal class PluginModel : CrossDomainObject
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private PluginExecutor _executor;


        public PluginConfig Config { get; private set; }

        public string InstanceId { get; }

        public AlgoPackageRef PackageRef { get; private set; }

        public AlgoPluginRef PluginRef { get; private set; }

        public PluginSetupModel Setup { get; private set; }

        public string FaultMessage { get; private set; }

        public PluginDescriptor Descriptor { get; private set; }

        public IAlgoPluginHost Host { get; }

        public PluginStates State { get; protected set; }


        protected LocalAlgoAgent Agent { get; }

        protected IAlgoSetupContext SetupContext { get; }


        public PluginModel(PluginConfig config, LocalAlgoAgent agent, IAlgoPluginHost host, IAlgoSetupContext setupContext)
        {
            Config = config;
            InstanceId = config.InstanceId;
            Agent = agent;
            Host = host;
            SetupContext = setupContext;

            UpdateRefs();

            Agent.Library.PluginUpdated += Library_PluginUpdated;
        }

        protected bool StartExcecutor()
        {
            if (PackageRef?.IsObsolete ?? true)
                UpdateRefs();
            if (State == PluginStates.Broken)
                return false;

            try
            {
                ChangeState(PluginStates.Starting);

                LockResources();
                Setup = new PluginSetupModel(PluginRef, Agent, SetupContext);
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
                ChangeState(PluginStates.Faulted, ex.Message);
                UnlockResources();

                return false;
            }
        }

        internal virtual void Configurate(PluginConfig config)
        {
            Config = config;
        }

        protected Task<bool> StopExecutor()
        {
            return Task.Factory.StartNew(() =>
            {
                ChangeState(PluginStates.Stopping);
                try
                {
                    _executor.Stop();
                    UnlockResources();
                    return true;
                }
                catch(Exception ex)
                {
                    _logger.Error(ex, "StopExcecutor() failed!");
                    ChangeState(PluginStates.Faulted, ex.Message);
                    UnlockResources();
                    return false;
                }
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
            executor.MainSymbolCode = Setup.MainSymbol.Id;
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

        protected virtual void HandleDisconnect()
        {
            _executor.HandleDisconnect();
        }

        protected virtual void ChangeState(PluginStates state, string faultMessage = null)
        {
            State = state;
            FaultMessage = faultMessage;
        }

        protected virtual void OnPluginUpdated()
        {
        }

        protected virtual void LockResources()
        {
            PackageRef.IncrementRef();
        }

        protected virtual void UnlockResources()
        {
            PackageRef?.DecrementRef();
        }

        protected void UpdateRefs()
        {
            var packageRef = Agent.Library.GetPackageRef(Config.Key.GetPackageKey());
            if (packageRef == null)
            {
                ChangeState(PluginStates.Broken, $"Package {Config.Key.PackageName} at {Config.Key.PackageLocation} is not found!");
                return;
            }
            var pluginRef = Agent.Library.GetPluginRef(Config.Key);
            if (pluginRef == null)
            {
                ChangeState(PluginStates.Broken, $"Plugin {Config.Key.DescriptorId} is missing in package {Config.Key.PackageName} at {Config.Key.PackageLocation}!");
                return;
            }

            PackageRef = packageRef;
            PluginRef = pluginRef;
            Descriptor = pluginRef.Metadata.Descriptor;
            ChangeState(PluginStates.Stopped);
        }

        private void Executor_OnRuntimeError(Exception ex)
        {
            _logger.Error(ex, "Exception in Algo executor! InstanceId=" + InstanceId);
        }

        private void Library_PluginUpdated(UpdateInfo<PluginInfo> update)
        {
            if (update.Type != UpdateType.Removed && update.Value.Key.Equals(Config.Key))
            {
                OnPluginUpdated();
            }
        }
    }
}
