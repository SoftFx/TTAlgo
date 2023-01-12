using System;
using System.Threading.Tasks;

namespace TickTrader.Algo.Async.Actors
{
    /*
     * Usage example
     * ActorMsg.For<IPluginHost>()
                .Tell(pluginId, static (host, id) => host.OnGlobalAlert(id))
                .SendTo(Self);
            ActorMsg.For<IPluginHost>()
                .Ask((pluginId, pluginId, default(PluginConfig)), static (host, state) => host.CreateExecutorConfig(state.Item1, state.Item2, state.Item3))
                .SendTo(Self);
     */

    public interface IActorMsg
    {
        void SendTo(IActorRef actor);
    }

    public interface IActorMsg<TRes>
    {
        Task<TRes> SendTo(IActorRef actor);
    }


    public interface IActorMsgBuilder<TModel>
    {
        IActorMsg Tell<TState>(TState state, Action<TModel, TState> action);

        IActorMsg<TRes> Ask<TState, TRes>(TState state, Func<TModel, TState, TRes> action);
    }


    public static class ActorMsg
    {
        public static IActorMsgBuilder<TModel> For<TModel>() => new ActorMsgBuilder<TModel>(); // TODO: Add caching by TModel
    }


    internal class ActorMsgBuilder<TModel> : IActorMsgBuilder<TModel>
    {
        public IActorMsg Tell<TState>(TState state, Action<TModel, TState> action) => new TellMsg<TState>(state);

        public IActorMsg<TRes> Ask<TState, TRes>(TState state, Func<TModel, TState, TRes> action) => new AskMsg<TState, TRes>(state);
    }


    internal class TellMsg<TState> : IActorMsg
    {
        public TState State { get; }

        public TellMsg(TState state) => State = state;

        public void SendTo(IActorRef actor) => actor.Tell(this);
    }


    internal class AskMsg<TState, TRes> : IActorMsg<TRes>
    {
        public TState State { get; }

        public AskMsg(TState state) => State = state;

        public Task<TRes> SendTo(IActorRef actor) => actor.Ask<TRes>(this);
    }
}
