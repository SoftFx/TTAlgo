using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Package;
using TickTrader.Algo.Rpc;
using TickTrader.Algo.Rpc.OverTcp;

namespace TickTrader.Algo.Server
{
    public class AlgoHostActor : Actor
    {
        public const string InternalApiAddress = "127.0.0.1";

        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<AlgoHostActor>();

        private readonly AlgoHostSettings _settings;
        private readonly HashSet<IActorRef> _consumers = new();

        private PackageStorage _pkgStorage;
        private RuntimeManager _runtimes;
        private RpcServer _internalApiServer;

        private MappingCollectionInfo _mappings;


        private AlgoHostActor(AlgoHostSettings settings)
        {
            _settings = settings;

            Receive<AlgoHostModel.StartCmd>(Start);
            Receive<AlgoHostModel.StopCmd>(Stop);
            Receive<AlgoHostModel.AddConsumerCmd>(AddConsumer);

            Receive<PackageUpdate>(upd => TellAllConsumers(upd));
            Receive<PackageStateUpdate>(upd => TellAllConsumers(upd));
            Receive<PackageVersionUpdate>(upd => OnPackageRefUpdate(upd));

            Receive<RuntimeServerModel.RuntimeRequest, IActorRef>(r => _runtimes.GetRuntime(r.Id));
            Receive<RuntimeServerModel.PkgRuntimeIdRequest, string>(r => _runtimes.GetPkgRuntimeId(r.PkgId));
            Receive<RuntimeServerModel.RuntimeStoppedMsg>(msg => _runtimes.OnRuntimeStopped(msg.Id));
            Receive<RuntimeServerModel.PkgRuntimeInvalidMsg>(OnPkgRuntimeInvalid);
            Receive<RuntimeServerModel.AccountControlRequest, IActorRef>(GetAccountControlInternal);

            Receive<AlgoHostModel.PkgFileExistsRequest, bool>(r => _pkgStorage.PackageFileExists(r.PkgName));
            Receive<AlgoHostModel.PkgBinaryRequest, byte[]>(r => _pkgStorage.GetPackageBinary(r.Id));
            Receive<AlgoHostModel.UploadPackageCmd, string>(cmd => _pkgStorage.UploadPackage(cmd.Request, cmd.FilePath));
            Receive<RemovePackageRequest>(r => _pkgStorage.RemovePackage(r));
            Receive<MappingsInfoRequest, MappingCollectionInfo>(_ => _mappings);
        }


        public static IActorRef Create(AlgoHostSettings settings)
        {
            return ActorSystem.SpawnLocal(() => new AlgoHostActor(settings), $"{nameof(AlgoHostActor)}");
        }


        protected override void ActorInit(object initMsg)
        {
            var reductions = new ReductionCollection();
            reductions.LoadDefaultReductions();
            _mappings = reductions.CreateMappings();

            _pkgStorage = new PackageStorage(Self);
            _runtimes = new RuntimeManager(Self, _settings.RuntimeSettings);

            _internalApiServer = new RpcServer(new TcpFactory(), new RpcResolver(Self));
        }


        private async Task Start(AlgoHostModel.StartCmd cmd)
        {
            _logger.Debug("Starting...");

            await _internalApiServer.Start(InternalApiAddress, 0);
            _logger.Info($"Started AlgoServer internal API on port {_internalApiServer.BoundPort}");

            _runtimes.ApiAddress = InternalApiAddress;
            _runtimes.ApiPort = _internalApiServer.BoundPort;

            await _pkgStorage.Start(_settings.PkgStorage);

            await _pkgStorage.WhenLoaded();

            _logger.Debug("Started");
        }

        private async Task Stop(AlgoHostModel.StopCmd cmd)
        {
            _logger.Debug("Stopping...");

            await _pkgStorage.Stop();

            await _runtimes.Shutdown();

            await _internalApiServer.Stop();

            _logger.Debug("Stopped");
        }

        public void AddConsumer(AlgoHostModel.AddConsumerCmd cmd)
        {
            _ = _consumers.Add(cmd.Ref);
        }

        private void TellAllConsumers(object msg)
        {
            foreach (var actor in _consumers)
                actor.Tell(msg);
        }

        private void OnPackageRefUpdate(PackageVersionUpdate update)
        {
            var pkgId = update.PkgId;
            var pkgRefId = update.LatestPkgRefId;

            _runtimes.MarkPkgRuntimeObsolete(pkgId);

            string runtimeId = null;
            if (!string.IsNullOrEmpty(pkgRefId))
            {
                runtimeId = CreatePkgRuntime(pkgId);
            }
            TellAllConsumers(new RuntimeControlModel.PkgRuntimeUpdateMsg(pkgId, runtimeId));
        }

        private void OnPkgRuntimeInvalid(RuntimeServerModel.PkgRuntimeInvalidMsg msg)
        {
            var pkgId = msg.PkgId;

            // Runtime can be marked obsolete after sending notification about invalid state
            // We need to check if sender runtime is still package runtime
            if (_runtimes.GetPkgRuntimeId(pkgId) == msg.RuntimeId)
            {
                _runtimes.MarkPkgRuntimeObsolete(pkgId);

                var runtimeId = CreatePkgRuntime(pkgId);
                TellAllConsumers(new RuntimeControlModel.PkgRuntimeUpdateMsg(pkgId, runtimeId));
            }
        }

        private string CreatePkgRuntime(string pkgId)
        {
            var pkgRef = _pkgStorage.GetPkgRef(pkgId);

            if (pkgRef == null)
            {
                _logger.Debug($"Skipped creating runtime for package '{pkgId}': no package");
                return null;
            }
            else if (!pkgRef.PkgInfo.IsValid)
            {
                _logger.Debug($"Skipped creating runtime for pkg ref '{pkgRef.Id}': invalid package");
                return null;
            }
            else
            {
                var runtimeId = _runtimes.CreatePkgRuntime(pkgRef);
                _logger.Debug($"Package '{pkgId}' current runtime: {runtimeId}");
                return runtimeId;
            }
        }

        private async Task<IActorRef> GetAccountControlInternal(RuntimeServerModel.AccountControlRequest request)
        {
            IActorRef accRef = default;
            var accId = request.Id;
            foreach(var actor in _consumers)
            {
                accRef = await actor.Ask<IActorRef>(request);
                if (accRef != null)
                    break;
            }

            if (accRef == null)
                throw Errors.AccountNotFound(accId);

            return accRef;
        }


        private sealed class RpcResolver : IRpcHost
        {
            private readonly IActorRef _host;


            public RpcResolver(IActorRef host)
            {
                _host = host;
            }


            ProtocolSpec IRpcHost.Resolve(ProtocolSpec protocol, out string error)
            {
                error = string.Empty;
                return protocol;
            }

            IRpcHandler IRpcHost.GetRpcHandler(ProtocolSpec protocol)
            {
                return protocol.Url switch
                {
                    KnownProtocolUrls.RuntimeV1 => new ServerRuntimeV1Handler(_host),
                    _ => null,
                };
            }
        }
    }
}
