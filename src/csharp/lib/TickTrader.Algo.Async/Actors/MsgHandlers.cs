using System;

namespace TickTrader.Algo.Async.Actors
{
    public class ActionHandler<T> : IMsgHandler
    {
        public static readonly string RequestType = typeof(T).FullName;

        private readonly Action<T> _handler;

        public string Type => RequestType;

        public ActionHandler(Action<T> handler)
        {
            _handler = handler;
        }

        public object Run(object msg)
        {
            if (msg is T typedMsg)
            {
                _handler(typedMsg);
                return null;
            }
            return Errors.InvalidMsgType(typeof(T), msg.GetType());
        }
    }

    public class FuncHandler<T, TRes> : IMsgHandler
    {
        public static readonly string RequestType = typeof(T).FullName;

        private readonly Func<T, TRes> _handler;

        public string Type => RequestType;


        public FuncHandler(Func<T, TRes> handler)
        {
            _handler = handler;
        }


        public object Run(object msg)
        {
            if (msg is T typedMsg)
            {
                return _handler(typedMsg);
            }
            return Errors.InvalidMsgType(typeof(T), msg.GetType());
        }
    }
}
