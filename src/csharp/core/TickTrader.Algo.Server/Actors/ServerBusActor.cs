using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    internal class ServerBusActor : Actor
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<ServerBusActor>();

        private readonly ServerSnapshotBuilder _snapshotBuilder = new ServerSnapshotBuilder();


        private ServerBusActor()
        {
            Receive<PackageUpdate>(OnPackageUpdate);
            Receive<PackageStateUpdate>(OnPackageStateUpdate);
            Receive<AccountModelUpdate>(OnAccountUpdate);
            Receive<AccountStateUpdate>(OnAccountStateUpdate);
            Receive<PluginModelUpdate>(OnPluginUpdate);
            Receive<PluginStateUpdate>(OnPluginStateUpdate);

            Receive<PackageSnapshotRequest, PackageListSnapshot>(_ => _snapshotBuilder.GetPackageSnapshot());
            Receive<AccountSnapshotRequest, AccountListSnapshot>(_ => _snapshotBuilder.GetAccountSnapshot());
            Receive<PluginSnapshotRequest, PluginListSnapshot>(_ => _snapshotBuilder.GetPluginSnapshot());
        }


        public static IActorRef Create()
        {
            return ActorSystem.SpawnLocal(() => new ServerBusActor(), $"{nameof(ServerBusActor)}");
        }


        private void OnPackageUpdate(PackageUpdate update)
        {
            _snapshotBuilder.UpdatePackage(update);
        }

        private void OnPackageStateUpdate(PackageStateUpdate update)
        {
            _snapshotBuilder.UpdatePackageState(update);
        }

        private void OnAccountUpdate(AccountModelUpdate update)
        {
            _snapshotBuilder.UpdateAccount(update);
        }

        private void OnAccountStateUpdate(AccountStateUpdate update)
        {
            _snapshotBuilder.UpdateAccountState(update);
        }

        private void OnPluginUpdate(PluginModelUpdate update)
        {
            _snapshotBuilder.UpdatePlugin(update);
        }

        private void OnPluginStateUpdate(PluginStateUpdate update)
        {
            _snapshotBuilder.UpdatePluginState(update);
        }


        internal class PackageSnapshotRequest : Singleton<PackageSnapshotRequest> { }

        internal class AccountSnapshotRequest : Singleton<AccountSnapshotRequest> { }

        internal class PluginSnapshotRequest : Singleton<PluginSnapshotRequest> { }
    }
}
