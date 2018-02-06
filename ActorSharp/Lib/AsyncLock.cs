using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace ActorSharp.Lib
{
    public class AsyncLock
    {
        private SynchronizationContext _context;
        private Queue<ReusableAwaitable<Token>> _queue = new Queue<ReusableAwaitable<Token>>();
        private Queue<ReusableAwaitable<Token>> _cahce = new Queue<ReusableAwaitable<Token>>();
        private bool _isTaken;

        public AsyncLock()
        {
            _context = SynchronizationContext.Current ?? throw new Exception("Synchronization context is required!");
        }

        public IAwaitable<Token> GetLock()
        {
            if (_isTaken)
            {
                var awaitable = GetFreeAwaitable();
                _queue.Enqueue(awaitable);
                return awaitable;
            }
            else
            {
                _isTaken = true;
                var awaitable = GetFreeAwaitable();
                awaitable.SetCompleted(new Token(this, awaitable));
                return awaitable;
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
            awaitable.Reset();
            _cahce.Enqueue(awaitable);
            if (_queue.Count > 0)
            {
                var next = _queue.Dequeue();
                //next.SetCompleted(new Token(this, next));
                _context.Post(c => next.SetCompleted(new Token(this, next)), null);
            }
            else
                _isTaken = false;
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
                _lock.Exit(_awaitable);
            }
        }
    }
}
