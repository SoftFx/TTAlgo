using Google.Protobuf;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    public class ServerBusModel
    {
        private readonly IActorRef _ref;


        public ServerBusModel(IActorRef actor)
        {
            _ref = actor;
        }


        public void SendUpdate(IMessage update) => _ref.Tell(update);

        public Task<PackageListSnapshot> GetPackageSnapshot() => _ref.Ask<PackageListSnapshot>(ServerBusActor.PackageSnapshotRequest.Instance);

        public Task<AccountListSnapshot> GetAccountSnapshot() => _ref.Ask<AccountListSnapshot>(ServerBusActor.AccountSnapshotRequest.Instance);

        public Task<PluginListSnapshot> GetPluginSnapshot() => _ref.Ask<PluginListSnapshot>(ServerBusActor.PluginSnapshotRequest.Instance);
    }
}
