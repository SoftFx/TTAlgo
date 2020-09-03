using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model.Config;
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


        public RuntimeV1Loader()
        {
            _client = new RpcClient(new TcpFactory(), this, new ProtocolSpec { Url = KnownProtocolUrls.RuntimeV1, MajorVerion = 1, MinorVerion = 0 });
            _handler = new UnitRuntimeV1Handler(this);
        }


        public async void Init(string address, int port, string proxyId)
        {
            CoreLoggerFactory.Init(n => new RuntimeLoggerAdapter(n));

            await _client.Connect(address, port).ConfigureAwait(false);
            await _handler.AttachRuntime(proxyId).ConfigureAwait(false);
        }

        public async void Deinit()
        {
            await _client.Disconnect("Runtime shutdown").ConfigureAwait(false);
        }

        public async Task Launch()
        {
            var runtimeConfig = await _handler.GetRuntimeConfig().ConfigureAwait(false);

            PluginConfig config;
            using (var stream = new MemoryStream(runtimeConfig.PluginConfig.Value.ToByteArray()))
            {
                var serializer = new DataContractSerializer(typeof(PluginConfig));
                config = (PluginConfig)serializer.ReadObject(stream);
            }

            var package = config.Key.GetPackageKey();
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

            var setupMetadata = new AlgoSetupMetadata(await _handler.GetSymbolListAsync(), mappings);
            var setupContext = new AlgoSetupContext(config.TimeFrame.ToDomainEnum(), config.MainSymbol);

            var setup = new PluginSetupModel(algoRef, setupMetadata, setupContext, config.MainSymbol);
            setup.Load(config);

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

            var provider = new RuntimeInfoProvider(_handler);

            _executorCore.ApplyConfig(executorConfig, provider, provider, provider, provider, provider, provider);

            var t = Task.Factory.StartNew(() => _executorCore.Start());
        }

        public Task Stop()
        {
            var t = Task.Factory.StartNew(() => _executorCore.Stop());
            return Task.CompletedTask;
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
