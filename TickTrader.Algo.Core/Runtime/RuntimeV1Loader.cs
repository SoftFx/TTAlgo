using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Container;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Rpc;
using TickTrader.Algo.Rpc.OverTcp;

namespace TickTrader.Algo.Core
{
    public class RuntimeV1Loader : CrossDomainObject, IRpcHost
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
            var path = await _handler.GetPackagePath(package.Name, (int)package.Location).ConfigureAwait(false);
            var sandbox = new AlgoSandbox(path, true);

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

            var setup = new PluginSetupModel(algoRef, null, null, config.MainSymbol);
            setup.Load(config);

            _executorCore = new PluginExecutorCore(config.Key.DescriptorId);
            _executorCore.OnNotification += msg => _handler.SendNotification(msg);

            _executorCore.IsGlobalMarshalingEnabled = true;
            _executorCore.IsBunchingRequired = true;

            var provider = new RuntimeInfoProvider(_handler);
            _executorCore.Metadata = provider;
            _executorCore.AccInfoProvider = provider;
            _executorCore.TradeExecutor = provider;
            _executorCore.TradeHistoryProvider = provider;
            _executorCore.Feed = provider;
            _executorCore.FeedHistory = provider;

            _executorCore.Start();
        }

        public void Stop()
        {
            _executorCore.Stop();
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
