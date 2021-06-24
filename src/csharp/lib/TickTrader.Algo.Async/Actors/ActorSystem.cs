using System;
using System.Collections.Concurrent;

namespace TickTrader.Algo.Async.Actors
{
    public class ActorSystemSettings
    {
        public IMsgDispatcherFactory MsgDispatcherFactory { get; set; } = new ChannelMsgDispatcherFactory();

        public int MaxBatch { get; set; } = 30;
    }

    public static class ActorSystem
    {
        private static readonly ConcurrentDictionary<string, Actor> _actors = new ConcurrentDictionary<string, Actor>();
        private static IMsgDispatcherFactory _msgDispatcherFactory;
        private static int _maxBatch;
        private static ChannelEventSource<ActorErrorException> _actorErrorSink = new ChannelEventSource<ActorErrorException>();
        private static ChannelEventSource<ActorFailedException> _actorFailedSink = new ChannelEventSource<ActorFailedException>();


        public static IEventSource<ActorErrorException> ActorErrors => _actorErrorSink;

        public static IEventSource<ActorFailedException> ActorFailed => _actorFailedSink;


        static ActorSystem()
        {
            var defaultSettings = new ActorSystemSettings();
            Init(defaultSettings);
        }

        public static void Init(ActorSystemSettings settings)
        {
            _msgDispatcherFactory = settings.MsgDispatcherFactory;
            _maxBatch = settings.MaxBatch;
        }


        internal static void OnActorError(string actorName, Exception ex)
        {
            _actorErrorSink.Writer.TryWrite(new ActorErrorException(actorName, ex));
        }

        internal static void OnActorFailed(string actorName, Exception ex)
        {
            _actorFailedSink.Writer.TryWrite(new ActorFailedException(actorName, ex));
        }


        public static IActorRef SpawnLocal<T>(string actorName = null, object initMsg = null)
            where T : Actor, new()
        {
            Actor instance = new T();
            actorName = actorName ?? typeof(T).FullName + Guid.NewGuid().ToString("N");
            
            if (!_actors.TryAdd(actorName, instance))
                throw Errors.DuplicateActorName(actorName);

            var msgDispather = _msgDispatcherFactory.CreateDispatcher(actorName, _maxBatch);
            instance.Init(actorName, msgDispather, initMsg);
            return instance.GetRef();
        }

        public static IActorRef SpawnLocal<T>(Func<T> factory, string actorName = null, object initMsg = null)
            where T : Actor
        {
            Actor instance = factory();
            actorName = actorName ?? typeof(T).FullName + Guid.NewGuid().ToString("N");

            if (!_actors.TryAdd(actorName, instance))
                throw Errors.DuplicateActorName(actorName);

            var msgDispather = _msgDispatcherFactory.CreateDispatcher(actorName, _maxBatch);
            instance.Init(actorName, msgDispather, initMsg ?? instance); // since we pass ctor params in factory method, we will most likely need to run some init code
            return instance.GetRef();
        }
    }
}
