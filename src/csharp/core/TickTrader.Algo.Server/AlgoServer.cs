using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Package;
using TickTrader.Algo.Rpc;
using TickTrader.Algo.Rpc.OverTcp;

namespace TickTrader.Algo.Server
{
    public class AlgoServer : IRpcHost
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<AlgoServer>();

        private readonly Dictionary<string, ExecutorModel> _executorsMap = new Dictionary<string, ExecutorModel>();
        private readonly Dictionary<string, IAccountProxy> _accountsMap = new Dictionary<string, IAccountProxy>();
        private readonly RpcServer _rpcServer;


        public string Address { get; } = "127.0.0.1";

        public int BoundPort => _rpcServer.BoundPort;

        public EnvService Env { get; }

        public PackageStorage PkgStorage { get; } = new PackageStorage();

        public AccountManagerModel Accounts { get; }

        public RuntimeManagerModel Runtimes { get; }

        public PluginManagerModel Plugins { get; }


        internal ServerStateModel SavedState { get; }


        public AlgoServer()
        {
            Env = new EnvService(AppDomain.CurrentDomain.BaseDirectory);

            Accounts = new AccountManagerModel(AccountManager.Create(this));
            Runtimes = new RuntimeManagerModel(RuntimeManager.Create(this));
            Plugins = new PluginManagerModel(PluginManager.Create(this));

            SavedState = new ServerStateModel(ServerStateManager.Create(Env.ServerStateFilePath));

            _rpcServer = new RpcServer(new TcpFactory(), this);
        }


        public async Task Start()
        {
            await _rpcServer.Start(Address, 0);

            await Plugins.Restore();
        }

        public async Task Stop()
        {
            _logger.Debug("Stopping...");

            await SavedState.StopSaving();

            await Plugins.Shutdown();

            await PkgStorage.Stop();

            await Runtimes.Shutdown();

            await _rpcServer.Stop();

            _logger.Debug("Stopped");
        }

        public async Task<ExecutorModel> CreateExecutor(string pkgId, string instanceId, ExecutorConfig config)
        {
            var runtime = await Runtimes.GetPkgRuntime(pkgId);
            if (runtime == null)
                throw new ArgumentException($"Package doesn't exists '{pkgId}'");
            await runtime.Start();

            return await runtime.CreateExecutor(instanceId, config);
        }

        public bool RegisterAccountProxy(IAccountProxy account)
        {
            if (_accountsMap.ContainsKey(account.Id))
                return false;

            _accountsMap.Add(account.Id, account);
            return true;
        }


        internal bool TryGetExecutor(string executorId, out ExecutorModel executor)
        {
            return _executorsMap.TryGetValue(executorId, out executor);
        }

        internal bool TryGetAccount(string accountId, out IAccountProxy account)
        {
            return _accountsMap.TryGetValue(accountId, out account);
        }

        internal void OnExecutorStopped(string executorId)
        {
            _executorsMap.Remove(executorId);
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
                    return new ServerRuntimeV1Handler(this);
            }
            return null;
        }

        #endregion IRpcHost implementation
    }
}
