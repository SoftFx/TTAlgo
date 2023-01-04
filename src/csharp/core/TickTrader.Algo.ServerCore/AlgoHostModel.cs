using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.Server
{
    public static class AlgoHostModel
    {
        public static Task Start(IActorRef actor) => actor.Ask(StartCmd.Instance);

        public static Task Stop(IActorRef actor) => actor.Ask(StopCmd.Instance);

        public static Task AddConsumer(IActorRef actor, IActorRef consumer) => actor.Ask(new AddConsumerCmd(consumer));


        public class StartCmd : Singleton<StartCmd> { }

        public class StopCmd : Singleton<StopCmd> { }

        public record AddConsumerCmd(IActorRef Ref);


        public record PkgFileExistsRequest(string PkgName);

        public record UploadPackageCmd(UploadPackageRequest Request, string FilePath);

        public record PkgBinaryRequest(string Id);
    }
}
