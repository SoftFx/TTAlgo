using NLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Api;

namespace TickTrader.BotTerminal
{
    internal class PluginModel : Algo.Core.Lib.CrossDomainObject, IPluginModel
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private PluginExecutor _executor;
        private Dictionary<string, IOutputCollector> _outputs;

        public PluginConfig Config { get; private set; }

        public string InstanceId { get; }

        public AlgoPackageRef PackageRef { get; private set; }

        public AlgoPluginRef PluginRef { get; private set; }

        public PluginSetupModel Setup { get; private set; }

        public string FaultMessage { get; private set; }

        public PluginDescriptor Descriptor { get; private set; }

        public IAlgoPluginHost Host { get; }

        public PluginStates State { get; protected set; }

        public IDictionary<string, IOutputCollector> Outputs => _outputs;

        protected LocalAlgoAgent Agent { get; }

        protected IAlgoSetupContext SetupContext { get; }

        public event Action OutputsChanged;

        public PluginModel(PluginConfig config, LocalAlgoAgent agent, IAlgoPluginHost host, IAlgoSetupContext setupContext)
        {
            Config = config;
            InstanceId = config.InstanceId;
            Agent = agent;
            Host = host;
            SetupContext = setupContext;

            _outputs = new Dictionary<string, IOutputCollector>();

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
                Setup.SetWorkingFolder(_executor.Config.WorkingFolder);
                Setup.Apply(_executor.Config);

                Host.UpdatePlugin(_executor);
                _executor.Start();
                //_executor.WriteConnectionInfo(Host.GetConnectionInfo());
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
                    _executor.WriteConnectionInfo(Host.GetConnectionInfo());
                    _executor.Stop();
                    ClearOutputs();
                    UnlockResources();
                    return true;
                }
                catch (Exception ex)
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
            var executor = new PluginExecutor(PluginRef, new DispatcherSync());

            executor.ErrorOccurred += Executor_OnRuntimeError;

            executor.Config.TimeFrame = Setup.SelectedTimeFrame;
            executor.Config.MainSymbolCode = Setup.MainSymbol.Id;
            executor.Config.InstanceId = InstanceId;
            executor.Config.Permissions = Setup.Permissions;
            executor.Config.WorkingFolder = EnvService.Instance.AlgoWorkingFolder;
            executor.Config.BotWorkingFolder = EnvService.Instance.AlgoWorkingFolder;
            executor.TradeHistoryProvider = Host.GetTradeHistoryApi();

            Host.InitializePlugin(executor);

            CreateOutputs(executor);

            return executor;
        }

        protected virtual void HandleReconnect()
        {
            _executor.HandleReconnect();
            _executor.WriteConnectionInfo(Host.GetConnectionInfo());
        }

        protected virtual void HandleDisconnect()
        {
            _executor.HandleDisconnect();
            _executor.WriteConnectionInfo(Host.GetConnectionInfo());
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

        protected virtual void OnRefsUpdated()
        {
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
            OnRefsUpdated();
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

        private void CreateOutputs(PluginExecutor executor)
        {
            try
            {
                foreach (var outputSetup in Setup.Outputs)
                {
                    if (outputSetup is ColoredLineOutputSetupModel)
                        CreateOuput<double>(executor, outputSetup);
                    else if (outputSetup is MarkerSeriesOutputSetupModel)
                        CreateOuput<Marker>(executor, outputSetup);
                }
                OutputsChanged?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to create outputs");
            }
        }

        private void CreateOuput<T>(PluginExecutor executor, OutputSetupModel setup)
        {
            executor.Config.SetupOutput<T>(setup.Id);
            var collector = CreateOutputCollector<T>(executor, setup);
            _outputs.Add(setup.Id, collector);
        }

        protected virtual IOutputCollector CreateOutputCollector<T>(PluginExecutor executor, OutputSetupModel setup)
        {
            return new OutputCollector<T>(setup, executor);
        }

        private void ClearOutputs()
        {
            try
            {
                _outputs.Values.Foreach(o => o.Dispose());
                _outputs.Clear();
                OutputsChanged?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to create outputs");
            }
        }
    }
}
