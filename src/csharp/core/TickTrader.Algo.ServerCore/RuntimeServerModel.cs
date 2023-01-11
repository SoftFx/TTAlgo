using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;

namespace TickTrader.Algo.Server
{
    public static class RuntimeServerModel
    {
        public static Task<IActorRef> GetRuntime(IActorRef actor, string runtimeId) => actor.Ask<IActorRef>(new RuntimeRequest(runtimeId));

        public static Task<string> GetPkgRuntimeId(IActorRef actor, string pkgId) => actor.Ask<string>(new PkgRuntimeIdRequest(pkgId));

        public static void OnRuntimeStopped(IActorRef actor, string runtimeId) => actor.Tell(new RuntimeStoppedMsg(runtimeId));

        public static void OnPkgRuntimeInvalid(IActorRef actor, string pkgId, string runtimeId) => actor.Tell(new PkgRuntimeInvalidMsg(pkgId, runtimeId));

        public static Task<IActorRef> GetAccountControl(IActorRef actor, string accId) => actor.Ask<IActorRef>(new AccountControlRequest(accId));


        public record RuntimeRequest(string Id);

        public record PkgRuntimeIdRequest(string PkgId);

        public record RuntimeStoppedMsg(string Id);

        public record PkgRuntimeInvalidMsg(string PkgId, string RuntimeId);

        public record AccountControlRequest(string Id);
    }
}
