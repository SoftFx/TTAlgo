using NLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;
using System.Linq;
using TickTrader.Algo.Protocol;

namespace TickTrader.BotTerminal
{
    internal class PluginModel : IPluginModel
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private ExecutorModel _executor;
        private Dictionary<string, IOutputCollector> _outputs;

        public PluginConfig Config { get; private set; }

        public string InstanceId { get; }

        public AlgoPackageRef PackageRef { get; private set; }

        public string FaultMessage { get; private set; }

        public PluginDescriptor Descriptor { get; private set; }

        public IAlgoPluginHost Host { get; }

        public PluginModelInfo.Types.PluginState State { get; protected set; }

        public IDictionary<string, IOutputCollector> Outputs => _outputs;

        protected LocalAlgoAgent Agent { get; }

        protected IAlgoSetupContext SetupContext { get; }

        protected IAlertModel AlertModel { get; }


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

            AlertModel = agent.AlertModel;
            //AlertUpdateEvent += agent.Shell.AlertsManager.UpdateAlertModel;
        }

        protected async Task<bool> StartExcecutor()
        {
            if (PackageRef?.IsObsolete ?? true)
                UpdateRefs();
            if (State == PluginModelInfo.Types.PluginState.Broken)
                return false;

            try
            {
                ChangeState(PluginModelInfo.Types.PluginState.Starting);

                LockResources();
                //Setup = new PluginSetupModel(PluginRef, Agent, SetupContext, Config.MainSymbol);
                //Setup.Load(Config);

                _executor = await CreateExecutor();
                //Setup.SetWorkingFolder(_executor.Config.WorkingFolder);
                //Setup.Apply(_executor.Config);

                Host.UpdatePlugin(_executor);
                await _executor.Start();
                //_executor.WriteConnectionInfo(Host.GetConnectionInfo());
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "StartExcecutor() failed!");
                ChangeState(PluginModelInfo.Types.PluginState.Faulted, ex.Message);
                UnlockResources();

                return false;
            }
        }

        internal virtual void Configurate(PluginConfig config)
        {
            Config = config;
        }

        protected async Task<bool> StopExecutor()
        {
            ChangeState(PluginModelInfo.Types.PluginState.Stopping);

            try
            {
                await _executor.Stop();
                ClearOutputs();
                UnlockResources();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "StopExcecutor() failed!");
                ChangeState(PluginModelInfo.Types.PluginState.Faulted, ex.Message);
                UnlockResources();
                return false;
            }
        }

        protected void AbortExecutor()
        {
            //_executor.Abort();
        }

        protected virtual async Task<ExecutorModel> CreateExecutor()
        {
            var runtime = await Agent.AlgoServer.CreateExecutor(Config, Agent.ClientModel.Id);

            runtime.ErrorOccurred += Executor_OnRuntimeError;

            runtime.Config.WorkingDirectory = EnvService.Instance.AlgoWorkingFolder;

            Host.InitializePlugin(runtime);

            CreateOutputs(runtime);

            return runtime;
        }

        protected virtual void HandleReconnect()
        {
            //_executor.NotifyReconnectNotification();
        }

        protected virtual void HandleDisconnect()
        {
            //_executor.NotifyDisconnectNotification();
        }

        protected virtual void ChangeState(PluginModelInfo.Types.PluginState state, string faultMessage = null)
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
            var package = Config.Key.Package;
            var packageRef = Agent.Library.GetPackageRef(package);
            if (packageRef == null)
            {
                ChangeState(PluginModelInfo.Types.PluginState.Broken, $"Algo Package {package.Name} at {package.Location} is not found!");
                return;
            }
            var plugin = Agent.Library.GetPlugin(Config.Key);
            if (plugin == null)
            {
                ChangeState(PluginModelInfo.Types.PluginState.Broken, $"Plugin {Config.Key.DescriptorId} is missing in Algo package {package.Name} at {package.Location}!");
                return;
            }

            PackageRef = packageRef;
            Descriptor = plugin.Descriptor_;
            ChangeState(PluginModelInfo.Types.PluginState.Stopped);
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

        private void CreateOutputs(ExecutorModel executor)
        {
            try
            {
                var descriptorLookup = Descriptor.Outputs.ToDictionary(d => d.Id);
                var properties = Config.UnpackProperties();
                foreach (IOutputConfig config in properties.Where(p => p is IOutputConfig))
                {
                    var descriptor = descriptorLookup[config.PropertyId];
                    if (config is ColoredLineOutputConfig)
                        CreateOuput<double>(executor, config, descriptor);
                    else if (config is MarkerSeriesOutputConfig)
                        CreateOuput<Marker>(executor, config, descriptor);
                }
                OutputsChanged?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to create outputs");
            }
        }

        private void CreateOuput<T>(ExecutorModel executor, IOutputConfig config, OutputDescriptor descriptor)
        {
            //executor.Config.SetupOutput<T>(setup.Id);
            var collector = CreateOutputCollector<T>(executor, config, descriptor);
            _outputs.Add(config.PropertyId, collector);
        }

        protected virtual IOutputCollector CreateOutputCollector<T>(ExecutorModel executor, IOutputConfig config, OutputDescriptor descriptor)
        {
            return new OutputCollector<T>(executor, config, descriptor);
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
