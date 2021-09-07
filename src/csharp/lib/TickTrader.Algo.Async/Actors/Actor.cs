using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.Algo.Async.Actors
{
    public class Actor
    {
        private readonly ConcurrentDictionary<string, IMsgHandler> _handlers = new ConcurrentDictionary<string, IMsgHandler>();

        private CancellationTokenSource _stopTokenSrc;
        private TaskScheduler _scheduler;

        public string Name { get; private set; }


        internal IMsgDispatcher MsgDispatcher { get; private set; }


        protected IActorRef Self { get; private set; }

        protected CancellationToken StopToken => _stopTokenSrc?.Token ?? throw Errors.ActorNotStarted(Name);

        protected TaskScheduler Scheduler => _scheduler ?? throw Errors.ActorNotStarted(Name);


        public ActorLock CreateLock()
        {
            if (MsgDispatcher == null)
                throw Errors.MsgDispatcherRequired();

            return new ActorLock(MsgDispatcher);
        }

        public ActorGate CreateGate()
        {
            if (MsgDispatcher == null)
                throw Errors.MsgDispatcherRequired();

            return new ActorGate(MsgDispatcher);
        }


        internal void Init(string name, IMsgDispatcher msgDispatcher, object initMsg = null)
        {
            Name = name ?? throw Errors.ActorNameRequired();
            MsgDispatcher = msgDispatcher ?? throw Errors.MsgDispatcherRequired();
            Self = new LocalRef(msgDispatcher, Name);
            _stopTokenSrc = new CancellationTokenSource();

            MsgDispatcher.Start(HandleMsg);
            MsgDispatcher.PostMessage(new InitCmd(initMsg));
        }

        internal async Task Stop()
        {
            if (MsgDispatcher == null)
                throw Errors.MsgDispatcherRequired();

            try
            {
                _stopTokenSrc.Cancel();
            }
            catch (Exception ex)
            {
                if (ex is AggregateException aggregateEx)
                {
                    foreach (var e in aggregateEx.InnerExceptions)
                    {
                        ActorSystem.OnActorError(Name, e);
                    }
                }
                else
                {
                    ActorSystem.OnActorError(Name, ex);
                }
            }

            await MsgDispatcher.Stop();
        }

        internal IActorRef GetRef() => Self;


        protected void Receive<T>(Action<T> action)
        {
            if (!_handlers.TryAdd(ActionHandler<T>.RequestType, new ActionHandler<T>(action)))
                ActorSystem.OnActorError(Name, Errors.DuplicateMsgHandler(typeof(T)));
        }

        protected void Receive<T, TRes>(Func<T, TRes> func)
        {
            if (!_handlers.TryAdd(FuncHandler<T, TRes>.RequestType, new FuncHandler<T, TRes>(func)))
                ActorSystem.OnActorError(Name, Errors.DuplicateMsgHandler(typeof(T)));
        }

        protected void Receive<T>(Func<T, Task> func) => Receive<T, Task>(func);

        protected void Receive<T, TRes>(Func<T, Task<TRes>> func) => Receive<T, Task<TRes>>(func);

        protected virtual void ActorInit(object initMsg) { }


        private void HandleMsg(object msg)
        {
            switch (msg)
            {
                case CallbackMsg callback:
                    InvokeCallback(callback);
                    break;
                case IAskMsg askMsg:
                    InvokeAskMsg(askMsg);
                    break;
                case ReusableAsyncToken token:
                    InvokeAsyncToken(token);
                    break;
                case InitCmd cmd:
                    InitInternal(cmd.InitMsg);
                    break;
                default:
                    InvokeMsg(msg);
                    break;
            }
        }

        private void InitInternal(object initMsg)
        {
            _scheduler = TaskScheduler.FromCurrentSynchronizationContext();

            if (initMsg != null)
            {
                try
                {
                    ActorInit(initMsg);
                }
                catch (Exception ex)
                {
                    ActorSystem.OnActorFailed(Name, ex);
                }
            }
        }

        private void InvokeCallback(CallbackMsg msg)
        {
            try
            {
                msg.Callback.Invoke(msg.State);
            }
            catch (Exception ex)
            {
                ActorSystem.OnActorError(Name, ex);
            }
        }

        private void InvokeAsyncToken(ReusableAsyncToken token)
        {
            try
            {
                token.SetCompleted();
            }
            catch (Exception ex)
            {
                ActorSystem.OnActorError(Name, ex);
            }
        }

        private void InvokeAskMsg(IAskMsg askMsg)
        {
            var response = InvokeMsgHandler(askMsg.Request.GetType().FullName, askMsg.Request);
            askMsg.SetResponse(response);
        }

        private void InvokeMsg(object msg)
        {
            var res = InvokeMsgHandler(msg.GetType().FullName, msg);
            if (res is Exception ex) // msg handlers can return exceptions as part of their logic
                ActorSystem.OnActorError(Name, ex);
        }

        private object InvokeMsgHandler(string msgType, object msg)
        {
            if (_handlers.TryGetValue(msgType, out var handler))
            {
                try
                {
                    return handler.Run(msg);
                }
                catch (Exception ex)
                {
                    return ex;
                }
            }

            return Errors.MsgHandlerNotFound(msgType);
        }


        private class InitCmd
        {
            public object InitMsg { get; }

            public InitCmd(object initMsg)
            {
                InitMsg = initMsg;
            }
        }
    }
}
