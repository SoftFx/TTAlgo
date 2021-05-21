using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace ActorSharp.Lib
{
    public class AsyncLock
    {
        private readonly SynchronizationContext _context;
        private readonly Queue<ReusableAwaitable<Token>> _queue = new Queue<ReusableAwaitable<Token>>();
        private readonly Queue<ReusableAwaitable<Token>> _cahce = new Queue<ReusableAwaitable<Token>>();
        private bool _isTaken;
        private readonly ReusableAwaitable<Token> _completed;

        public AsyncLock()
        {
            _context = SynchronizationContext.Current ?? throw new Exception("Synchronization context is required!");
            _completed = ReusableAwaitable<Token>.CreateCompleted(new Token(this, null));
        }

        public string TakenBy { get; private set; }

        public IAwaitable<Token> GetLock(string lockerName = null)
        {
            ContextCheck();

            if (_isTaken)
            {
                var awaitable = GetFreeAwaitable();
                awaitable.WaiterName = lockerName;
                _queue.Enqueue(awaitable);
                return awaitable;
            }
            else
            {
                _isTaken = true;
                TakenBy = lockerName;
                return _completed;
            }
        }

        private ReusableAwaitable<Token> GetFreeAwaitable()
        {
            if (_cahce.Count > 0)
                return _cahce.Dequeue();
            else
                return new ReusableAwaitable<Token>();
        }

        private void Exit(ReusableAwaitable<Token> awaitable)
        {
            if (awaitable != null)
            {
                awaitable.Reset();
                _cahce.Enqueue(awaitable);
            }

            if (_queue.Count > 0)
            {
                var next = _queue.Dequeue();
                TakenBy = next.WaiterName;
                //next.SetCompleted(new Token(this, next));
                _context.Post(c => next.SetCompleted(new Token(this, next)), null);
            }
            else
            {
                TakenBy = null;
                _isTaken = false;
            }
        }

        protected void ContextCheck()
        {
            #if DEBUG
            if (SynchronizationContext.Current != _context)
                throw new Exception("Synchronization violation! You cannot directly access this object from another context!");
            #endif
        }

        public struct Token : IDisposable
        {
            private AsyncLock _lock;
            private ReusableAwaitable<Token> _awaitable;

            internal Token(AsyncLock lockObj, ReusableAwaitable<Token> awaitable)
            {
                _lock = lockObj;
                _awaitable = awaitable;
            }

            public void Dispose()
            {
                _lock.ContextCheck();
                _lock.Exit(_awaitable);
            }
        }
    }
}
