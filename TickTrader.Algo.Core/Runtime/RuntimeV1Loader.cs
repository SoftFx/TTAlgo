using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Container;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;
using TickTrader.Algo.Rpc.OverTcp;

namespace TickTrader.Algo.Core
{
    public class RuntimeV1Loader : CrossDomainObject, IRpcHost, IRuntimeProxy
    {
        public const int AbortTimeout = 10000;


        private readonly RpcClient _client;
        private readonly UnitRuntimeV1Handler _handler;
        private TaskCompletionSource<bool> _finishTaskSrc;
        private RuntimeConfig _runtimeConfig;
        private AlgoSandbox _sandbox;
        private Dictionary<string, PluginExecutorCore> _executorsMap;


        public RuntimeV1Loader()
        {
            _client = new RpcClient(new TcpFactory(), this, new ProtocolSpec { Url = KnownProtocolUrls.RuntimeV1, MajorVerion = 1, MinorVerion = 0 });
            _handler = new UnitRuntimeV1Handler(this);

            _executorsMap = new Dictionary<string, PluginExecutorCore>();
        }


        public async void Init(string address, int port, string proxyId)
        {
            await _client.Connect(address, port).ConfigureAwait(false);
            await _handler.AttachRuntime(proxyId).ConfigureAwait(false);
        }

        public async void Deinit()
        {
            await _client.Disconnect("Runtime shutdown").ConfigureAwait(false);
        }

        public Task WhenFinished()
        {
            if (_finishTaskSrc == null)
                _finishTaskSrc = new TaskCompletionSource<bool>();

            return _finishTaskSrc.Task;
        }

        public async Task Launch()
        {
            _runtimeConfig = await _handler.GetRuntimeConfig().ConfigureAwait(false);

            _sandbox = new AlgoSandbox(_runtimeConfig.PackagePath, false);

            //var package = config.Key.Package;
            //var path = string.Empty;
            //if (package.Location == RepositoryLocation.Embedded && package.Name.Equals("TickTrader.Algo.Indicators.dll", System.StringComparison.OrdinalIgnoreCase))
            //{
            //    var indicatorsAssembly = Assembly.Load("TickTrader.Algo.Indicators");
            //    AlgoAssemblyInspector.FindPlugins(indicatorsAssembly);
            //    path = indicatorsAssembly.Location;
            //}
            //else
            //{
            //    path = await _handler.GetPackagePath(package.Name, (int)package.Location).ConfigureAwait(false);
            //    var sandbox = new AlgoSandbox(path, false);
            //}

            //var requiredPackages = new List<PackageKey> { config.Key.GetPackageKey() };
            //requiredPackages.AddRange(config.SelectedMapping.GetPackageKeys());
            //foreach (var property in config.Properties)
            //{
            //    var input = property as MappedInput;
            //    if (input != null)
            //    {
            //        requiredPackages.AddRange(input.SelectedMapping.GetPackageKeys());
            //    }
            //}

            //foreach(var package in requiredPackages.Distinct())
            //{
            //    var path = await _handler.GetPackagePath(package.Name, (int)package.Location);
            //    var sandbox = new AlgoSandbox(path, true);
            //}
        }

        public async Task Stop()
        {
            try
            {
                var stopTasks = _executorsMap.Values.Select(e => StopExecutor(e));
                await Task.WhenAll(stopTasks);

                _finishTaskSrc?.TrySetResult(true);
            }
            catch (Exception)
            {
                _finishTaskSrc?.TrySetResult(false);
            }
        }

        public async Task StartExecutor(string executorId)
        {
            if (_executorsMap.TryGetValue(executorId, out var executorCore))
                throw new Exception("Executor already started");

            var executorConfig = await _handler.GetExecutorConfig(executorId).ConfigureAwait(false);

            var config = executorConfig.PluginConfig.Unpack<PluginConfig>();

            var metadata = AlgoAssemblyInspector.GetPlugin(config.Key.DescriptorId);
            var algoRef = new AlgoPluginRef(metadata, _runtimeConfig.PackagePath);

            var reductions = new ReductionCollection(CoreLoggerFactory.GetLogger("Extensions"));
            var mappings = new MappingCollection(reductions);

            var accountProxy = await _handler.AttachAccount(executorConfig.AccountId);

            var setupMetadata = new AlgoSetupMetadata(accountProxy.Metadata.GetSymbolMetadata(), mappings);
            var setupContext = new AlgoSetupContext(config.Timeframe, config.MainSymbol);

            var setup = new PluginSetupModel(algoRef, setupMetadata, setupContext, config.MainSymbol);
            setup.Load(config);
            setup.SetWorkingFolder(executorConfig.WorkingDirectory);

            var coreExecutorConfig = new PluginExecutorConfig();
            coreExecutorConfig.LoadFrom(executorConfig, config);
            setup.Apply(coreExecutorConfig);
            foreach (var outputSetup in setup.Outputs)
            {
                if (outputSetup is ColoredLineOutputSetupModel)
                    coreExecutorConfig.SetupOutput<double>(outputSetup.Id);
                else if (outputSetup is MarkerSeriesOutputSetupModel)
                    coreExecutorConfig.SetupOutput<Api.Marker>(outputSetup.Id);
            }
            if (executorConfig.MainSeries?.Is(BarChunk.Descriptor) ?? false)
            {
                var bars = executorConfig.MainSeries.Unpack<BarChunk>();
                coreExecutorConfig.GetFeedStrategy<BarStrategy>().SetMainSeries(bars.Bars.ToList());
            }

            executorCore = new PluginExecutorCore(config.Key.DescriptorId);
            executorCore.OnNotification += msg => _handler.SendNotification(executorId, msg);

            executorCore.IsGlobalMarshalingEnabled = true;
            executorCore.IsBunchingRequired = true;

            executorCore.AccountId = executorConfig.AccountId;

            executorCore.ApplyConfig(coreExecutorConfig, accountProxy);

            _executorsMap.Add(executorId, executorCore);

            var t = Task.Factory.StartNew(() => executorCore.Start());
        }

        public Task StopExecutor(string executorId)
        {
            if (!_executorsMap.TryGetValue(executorId, out var executorCore))
                throw new ArgumentException("Unknown executorId");

            return StopExecutor(executorCore);
        }


        internal void InitDebugLogger()
        {
            CoreLoggerFactory.Init(n => new DebugLogger(n));
        }


        private async Task StopExecutor(PluginExecutorCore executorCore)
        {
            var stopTask = executorCore.Stop();
            var delayTask = Task.Delay(AbortTimeout);
            var t = await Task.WhenAny(stopTask, delayTask);
            if (t == delayTask)
            {
                executorCore.Abort();
            }

            executorCore.Dispose();
            _executorsMap.Remove(executorCore.InstanceId);

            await _handler.DetachAccount(executorCore.AccountId);
        }


        #region IRpcHost implementation

        ProtocolSpec IRpcHost.Resolve(ProtocolSpec protocol, out string error)
        {
            error = string.Empty;
            return protocol;
        }

        IRpcHandler IRpcHost.GetRpcHandler(ProtocolSpec protocol)
        {
            switch (protocol.Url)
            {
                case KnownProtocolUrls.RuntimeV1:
                    return _handler;
            }
            return null;
        }

        #endregion IRpcHost implementation
    }
}
