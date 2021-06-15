using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace TickTrader.Algo.Async.Actors
{
    public class Actor
    {
        private readonly ConcurrentDictionary<string, IMsgHandler> _handlers = new ConcurrentDictionary<string, IMsgHandler>();

        public string Name { get; private set; }


        internal IMsgDispatcher MsgDispatcher { get; private set; }


        internal void Init(string name, IMsgDispatcher msgDispatcher, object initMsg = null)
        {
            Name = name ?? throw Errors.ActorNameRequired();
            MsgDispatcher = msgDispatcher ?? throw Errors.MsgDispatcherRequired();
            MsgDispatcher.Start(HandleMsg);
            if (initMsg != null)
                MsgDispatcher.PostMessage(new InvokeInitCmd(initMsg));
        }


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
                case InvokeInitCmd cmd:
                    InvokeInit(cmd.InitMsg);
                    break;
                default:
                    InvokeMsg(msg);
                    break;
            }
        }

        private void InvokeInit(object initMsg)
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

        private void InvokeAskMsg(IAskMsg askMsg)
        {
            try
            {
                var response = InvokeMsgHandler(askMsg.Request.GetType().FullName, askMsg.Request);
                askMsg.SetResponse(response);
            }
            catch (Exception ex)
            {
                askMsg.SetResponse(ex);
            }
        }

        private void InvokeMsg(object msg)
        {
            var res = InvokeMsgHandler(msg.GetType().FullName, msg);
            if (res is Exception ex)
                ActorSystem.OnActorError(Name, ex);
        }

        private object InvokeMsgHandler(string msgType, object msg)
        {
            if (_handlers.TryGetValue(msgType, out var handler))
                return handler.Run(msg);

            return Errors.MsgHandlerNotFound(msgType);
        }


        private class InvokeInitCmd
        {
            public object InitMsg { get; }

            public InvokeInitCmd(object initMsg)
            {
                InitMsg = initMsg;
            }
        }
    }
}
