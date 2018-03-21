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
            ContextCheck();

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
            ContextCheck();

            if (!_isClosed)
            {
                _isClosed = true;
                if (clearQueue)
                    _innerQueue.Clear();
                if (_callback != null)
                    _context.Post(s => SignalAwaitable(), null);
                //SignalAwaitable();
            }
        }

        public IAwaitable<bool> Dequeue()
        {
            ContextCheck();

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

        protected void ContextCheck()
        {
            #if DEBUG
            if (SynchronizationContext.Current != _context)
                throw new Exception("Synchronization violation! You cannot directly access this object from another context!");
            #endif
        }

        #region IAwaitable (read)

        bool IAwaiter<bool>.IsCompleted
        {
            get
            {
                ContextCheck();
                return _innerQueue.Count > 0 || _isClosed;
            }
        }

        IAwaiter<bool> IAwaitable<bool>.GetAwaiter()
        {
            ContextCheck();

            return this;
        }

        bool IAwaiter<bool>.GetResult()
        {
            ContextCheck();

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
