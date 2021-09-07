using System;

namespace TickTrader.Algo.Isolation.NetFx
{
    public interface ICallback<T>
    {
        void Invoke(T arg);
    }

    public interface ICallback
    {
        void Invoke();
    }

    public class CrossDomainCallback<T> : CrossDomainObject, ICallback<T>
    {
        public CrossDomainCallback()
        {
        }

        public Action<T> Action { get; set; }

        public CrossDomainCallback(Action<T> callbackAction)
        {
            Action = callbackAction;
        }

        public void Invoke(T args)
        {
            Action(args);
        }
    }

    public class CrossDomainCallback : CrossDomainObject, ICallback
    {
        public CrossDomainCallback()
        {
        }

        public Action Action { get; set; }

        public CrossDomainCallback(Action callbackAction)
        {
            Action = callbackAction;
        }

        public void Invoke()
        {
            Action();
        }
    }
}
