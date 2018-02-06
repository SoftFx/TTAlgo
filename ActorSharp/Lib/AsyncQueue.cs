using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace ActorSharp.Lib
{
    public class AsyncQueue<T> : IAwaitable<bool>, IAwaiter<bool>
    {
        private SynchronizationContext _context;
        private Queue<T> _innerQueue = new Queue<T>();
        private Action _callback;
        private bool _isClosed;

        public AsyncQueue()
        {
            _context = SynchronizationContext.Current ?? throw new Exception("Synchronization context is required!");
        }

        public void Enqueue(T item)
        {
            if (_isClosed)
                throw new InvalidOperationException("Queue is closed!");

            _innerQueue.Enqueue(item);
            SignalAwaitable();
        }

        public int Count => _innerQueue.Count;
        public T Item { get; private set; }
        public bool IsClosed => _isClosed;

        public void Close(bool clearQueue = false)
        {
            if (!_isClosed)
            {
                _isClosed = true;
                if (clearQueue)
                    _innerQueue.Clear();
                //SignalAwaitable();
                _context.Post(s => SignalAwaitable(), null);
            }
        }

        public IAwaitable<bool> Dequeue()
        {
            #region DEBUG
            if (_callback != null)
                throw new InvalidOperationException("AsyncQueue is busy with another async operation!");
            #endregion

            return this;
        }

        private void SignalAwaitable()
        {
            if (_callback != null)
            {
                var toInvoke = _callback;
                _callback = null;
                toInvoke.Invoke();
            }
        }

        #region IAwaitable (read)

        bool IAwaiter<bool>.IsCompleted => _innerQueue.Count > 0 || _isClosed;

        IAwaiter<bool> IAwaitable<bool>.GetAwaiter()
        {
            return this;
        }

        bool IAwaiter<bool>.GetResult()
        {
            if (IsClosed && _innerQueue.Count == 0)
                return false;

            Item = _innerQueue.Dequeue();
            return true;
        }

        void INotifyCompletion.OnCompleted(Action continuation)
        {
            #region DEBUG
            if (_callback != null)
                throw new InvalidOperationException("AsyncQueue is busy with another async operation!");
            #endregion

            _callback = continuation;
        }

        #endregion
    }
}
