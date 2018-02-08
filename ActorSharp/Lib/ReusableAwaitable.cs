using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ActorSharp.Lib
{
    internal class ReusableAwaitable : IAwaitable, IAwaiter
    {
        private static readonly IAwaitable completedSingleton = new ReusableAwaitable() { _isCompleted = true };

        private bool _isCompleted;
        private Action _callback;

        public static IAwaitable Completed => completedSingleton;

        public void Reset()
        {
            _isCompleted = false;
            _callback = null;
        }

        public void SetCompleted()
        {
            _isCompleted = true;
            _callback?.Invoke();
        }

        bool IAwaiter.IsCompleted => _isCompleted;

        IAwaiter IAwaitable.GetAwaiter()
        {
            return this;
        }

        void IAwaiter.GetResult()
        {
        }

        void INotifyCompletion.OnCompleted(Action continuation)
        {
            _callback = continuation;
        }
    }

    internal class ReusableAwaitable<T> : IAwaitable<T>, IAwaiter<T>
    {
        private T _result;
        private bool _isCompleted;
        private Action _callback;

        public void Reset()
        {
            _result = default(T);
            _isCompleted = false;
            _callback = null;
        }

        public void SetCompleted(T result)
        {
            _result = result;
            _isCompleted = true;
            _callback?.Invoke();
        }

        bool IAwaiter<T>.IsCompleted => _isCompleted;

        IAwaiter<T> IAwaitable<T>.GetAwaiter()
        {
            return this;
        }

        T IAwaiter<T>.GetResult()
        {
            return _result;
        }

        void INotifyCompletion.OnCompleted(Action continuation)
        {
            _callback = continuation;
        }
    }
}
