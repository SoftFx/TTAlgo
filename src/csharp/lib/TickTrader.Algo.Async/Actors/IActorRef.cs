using System.Threading.Tasks;

namespace TickTrader.Algo.Async.Actors
{
    public interface IActorRef
    {
        string Name { get; }


        void Tell(object message);
    }


    public static class ActorRefExtensions
    {
        public static Task Ask(this IActorRef actor, object request)
        {
            var askMsg = new AskMsg(request);
            actor.Tell(askMsg);
            return askMsg.Task;
        }

        public static Task<TResponse> Ask<TResponse>(this IActorRef actor, object request)
        {
            var askMsg = new AskMsg<TResponse>(request);
            actor.Tell(askMsg);
            return askMsg.Task;
        }
    }
}
