using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;
using TickTrader.Algo.Rpc.OverTcp;
using TickTrader.Algo.Util;

namespace TickTrader.Algo.Core
{
    public class AlgoServer : IRpcHost
    {
        private static readonly IAlgoCoreLogger _logger = CoreLoggerFactory.GetLogger<AlgoServer>();

        private readonly Dictionary<string, int> _packageVersions;
        private readonly Dictionary<string, AlgoPackageRef> _packagesMap;
        private readonly Dictionary<string, RuntimeModel> _runtimesMap;
        private readonly Dictionary<string, ExecutorModel> _executorsMap;
        private readonly Dictionary<string, IAccountProxy> _accountsMap;
        private readonly RpcServer _rpcServer;
        private readonly AsyncChannelProcessor<PackageFileUpdate> _packageProcessor;
        private Dictionary<RepositoryLocation, PackageRepository> _repositories;


        public string Address { get; } = "127.0.0.1";

        public int BoundPort => _rpcServer.BoundPort;


        public AlgoServer()
        {
            _packageVersions = new Dictionary<string, int>();
            _packagesMap = new Dictionary<string, AlgoPackageRef>();
            _runtimesMap = new Dictionary<string, RuntimeModel>();
            _executorsMap = new Dictionary<string, ExecutorModel>();
            _accountsMap = new Dictionary<string, IAccountProxy>();

            _packageProcessor = AsyncChannelProcessor<PackageFileUpdate>.CreateUnbounded($"{nameof(AlgoServer)} package loop", true);
            _repositories = new Dictionary<RepositoryLocation, PackageRepository>();

            _rpcServer = new RpcServer(new TcpFactory(), this);
        }


        public async Task Start()
        {
            await _rpcServer.Start(Address, 0);
            _packageProcessor.Start(HandlePackageUpdate);
        }

        public async Task Stop()
        {
            await _packageProcessor.Stop(false);
            await Task.WhenAll(_repositories.Values.Select(r => r.Stop()));
            await Task.WhenAll(_runtimesMap.Values.Select(r => StopRuntime(r)));
            await _rpcServer.Stop();
        }

        public RuntimeModel CreateRuntime(AlgoPluginRef pluginRef, ISyncContext updatesSync)
        {
            var id = Guid.NewGuid().ToString("N");
            var runtime = new RuntimeModel(this, id, pluginRef.PackagePath);
            _runtimesMap.Add(id, runtime);
            return runtime;
        }

        public async Task<ExecutorModel> CreateExecutor(PluginConfig config, string accountId)
        {
            if (_executorsMap.ContainsKey(config.InstanceId))
                throw new ArgumentException("Duplicate instance id");

            var packageId = PackageHelper.GetPackageId(config.Key.Package);

            var runtime = await GetActiveRuntime(packageId);
            var executor = runtime.CreateExecutor(config, accountId);
            _executorsMap.Add(executor.Id, executor);
            return executor;
        }

        public bool RegisterAccountProxy(IAccountProxy account)
        {
            if (_accountsMap.ContainsKey(account.Id))
                return false;

            _accountsMap.Add(account.Id, account);
            return true;
        }

        public void RegisterPackageRepository(RepositoryLocation location, string path)
        {
            if (_repositories.ContainsKey(location))
                throw new ArgumentException($"Cannot register multiple paths for location '{location}'");

            var repo = new PackageRepository(path, location, _packageProcessor, CoreLoggerFactory.GetLogger<PackageRepository>(), true);
            _repositories.Add(location, repo);
            repo.Start();
        }

        public void AddAssemblyAsPackage(string assemblyPath)
        {
            var key = PackageHelper.GetPackageKey(RepositoryLocation.Embedded, assemblyPath);
            var id = PackageHelper.GetPackageId(key);
            _packageProcessor.Add(new PackageFileUpdate(id, key, Repository.UpdateAction.Upsert, assemblyPath));
        }


        internal bool TryGetRuntime(string runtimeId, out RuntimeModel runtime)
        {
            return _runtimesMap.TryGetValue(runtimeId, out runtime);
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


        private string GenerateRuntimeId(string packageId)
        {
            if (!_packageVersions.TryGetValue(packageId, out var currentVersion))
                currentVersion = -1;

            currentVersion++;
            _packageVersions[packageId] = currentVersion;
            return $"{packageId}/{currentVersion}";
        }

        private async Task<RuntimeModel> GetActiveRuntime(string packageId)
        {
            if (!_packagesMap.TryGetValue(packageId, out var package))
                throw new ArgumentException("Package not found", nameof(packageId));

            var runtime = package.ActiveRuntime;
            await runtime.WaitForLaunch();
            return runtime;
        }

        private async Task StopRuntime(RuntimeModel runtime)
        {
            try
            {
                await runtime.Stop();
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to stop runtime {runtime.Id}", ex);
            }
        }

        private async Task HandlePackageUpdate(PackageFileUpdate update)
        {
            var packageId = update.PackageId;
            var hasPackageRef = _packagesMap.TryGetValue(packageId, out var packageRef);

            switch (update.Action)
            {
                case Repository.UpdateAction.Upsert:
                    var runtimeId = GenerateRuntimeId(packageId);
                    var runtime = new RuntimeModel(this, runtimeId, update.FilePath);
                    _runtimesMap[runtimeId] = runtime;

                    await runtime.Start(Address, BoundPort);

                    var package = await runtime.GetPackageInfo();
                    package.Key.Location = update.PackageKey.Location; // temp fix
                    foreach (var p in package.Plugins) p.Key.Package.Location = update.PackageKey.Location;

                    if (!hasPackageRef)
                    {
                        packageRef = new AlgoPackageRef(package);
                        _packagesMap[packageId] = packageRef;
                    }
                    packageRef.Update(runtime, package);

                    PackageUpdated?.Invoke(PackageUpdate.Upsert(packageId, package));

                    break;
                case Repository.UpdateAction.Remove:
                    if (hasPackageRef)
                    {
                        //activeRuntime.MarkObsolete();
                        _packagesMap.Remove(packageId);
                        PackageUpdated?.Invoke(PackageUpdate.Remove(packageId));
                    }
                    break;
            }
        }

        public bool TryGetPackage(string packageId, out AlgoPackageRef packageRef)
        {
            return _packagesMap.TryGetValue(packageId, out packageRef);
        }

        public event Action<PackageUpdate> PackageUpdated;


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
