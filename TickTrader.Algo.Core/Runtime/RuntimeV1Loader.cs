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
        private readonly RpcClient _client;
        private readonly UnitRuntimeV1Handler _handler;
        private PluginExecutorCore _executorCore;
        private TaskCompletionSource<bool> _finishTaskSrc;


        public RuntimeV1Loader()
        {
            _client = new RpcClient(new TcpFactory(), this, new ProtocolSpec { Url = KnownProtocolUrls.RuntimeV1, MajorVerion = 1, MinorVerion = 0 });
            _handler = new UnitRuntimeV1Handler(this);
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
            var runtimeConfig = await _handler.GetRuntimeConfig().ConfigureAwait(false);

            var config = runtimeConfig.PluginConfig.Unpack<PluginConfig>();

            var package = config.Key.Package;
            var path = string.Empty;
            if (package.Location == RepositoryLocation.Embedded && package.Name.Equals("TickTrader.Algo.Indicators.dll", System.StringComparison.OrdinalIgnoreCase))
            {
                var indicatorsAssembly = Assembly.Load("TickTrader.Algo.Indicators");
                AlgoAssemblyInspector.FindPlugins(indicatorsAssembly);
                path = indicatorsAssembly.Location;
            }
            else
            {
                path = await _handler.GetPackagePath(package.Name, (int)package.Location).ConfigureAwait(false);
                var sandbox = new AlgoSandbox(path, false);
            }

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

            var metadata = AlgoAssemblyInspector.GetPlugin(config.Key.DescriptorId);
            var algoRef = new AlgoPluginRef(metadata, path);

            var reductions = new ReductionCollection(CoreLoggerFactory.GetLogger("Extensions"));
            var mappings = new MappingCollection(reductions);

            var provider = new RuntimeInfoProvider(_handler);
            await provider.PreLoad();

            var setupMetadata = new AlgoSetupMetadata(provider.GetSymbolMetadata(), mappings);
            var setupContext = new AlgoSetupContext(config.Timeframe, config.MainSymbol);

            var setup = new PluginSetupModel(algoRef, setupMetadata, setupContext, config.MainSymbol);
            setup.Load(config);
            setup.SetWorkingFolder(runtimeConfig.WorkingDirectory);

            _executorCore = new PluginExecutorCore(config.Key.DescriptorId);
            _executorCore.OnNotification += msg => _handler.SendNotification(msg);

            _executorCore.IsGlobalMarshalingEnabled = true;
            _executorCore.IsBunchingRequired = true;

            var executorConfig = new PluginExecutorConfig();
            executorConfig.LoadFrom(runtimeConfig, config);
            setup.Apply(executorConfig);
            foreach (var outputSetup in setup.Outputs)
            {
                if (outputSetup is ColoredLineOutputSetupModel)
                    executorConfig.SetupOutput<double>(outputSetup.Id);
                else if (outputSetup is MarkerSeriesOutputSetupModel)
                    executorConfig.SetupOutput<Api.Marker>(outputSetup.Id);
            }
            if (runtimeConfig.MainSeries?.Is(BarChunk.Descriptor) ?? false)
            {
                var bars = runtimeConfig.MainSeries.Unpack<BarChunk>();
                executorConfig.GetFeedStrategy<BarStrategy>().SetMainSeries(bars.Bars.ToList());
            }

            _executorCore.ApplyConfig(executorConfig, provider, provider, provider, provider, provider, provider);

            var t = Task.Factory.StartNew(() => _executorCore.Start());
        }

        public async Task Stop()
        {
            await _executorCore.Stop();
            _finishTaskSrc?.TrySetResult(true);
        }


        internal void InitDebugLogger()
        {
            CoreLoggerFactory.Init(n => new DebugLogger(n));
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
