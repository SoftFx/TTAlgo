using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ActorSharp.Lib
{
    public class AsyncGate
    {
        private readonly SynchronizationContext _context;
        private readonly Queue<ReusableAwaitable<GateToken>> _queue = new Queue<ReusableAwaitable<GateToken>>();
        private readonly Queue<ReusableAwaitable<GateToken>> _cahce = new Queue<ReusableAwaitable<GateToken>>();
        private readonly ReusableAwaitable<GateToken> _completed;
        private readonly ReusableAwaitable _exitAwaitable;
        private int _enteredCount;
        private bool _isOpened;

        public AsyncGate()
        {
            _context = SynchronizationContext.Current ?? throw new Exception("Synchronization context is required!");
            _completed = ReusableAwaitable<GateToken>.CreateCompleted(new GateToken(this, null));
            _exitAwaitable = new ReusableAwaitable();
        }

        public int WatingCount => _queue.Count;
        public int EnteredCount => _enteredCount;

        public event Action OnWait;
        public event Action OnExit;

        public void Open()
        {
            if (_isOpened)
                throw new InvalidOperationException("Gate is already opened!");

            if(_enteredCount > 0)
                throw new InvalidOperationException("Gate is not yet closed!");

            _isOpened = true;
            _exitAwaitable.Reset();

            while (_queue.Count > 0)
            {
                var waiter = _queue.Dequeue();
                _context.Post(c => waiter.SetCompleted(new GateToken(this, waiter)), null);
                _enteredCount++;
            }
        }

        public IAwaitable Close()
        {
            if(!_isOpened)
                throw new InvalidOperationException("Gate is already closed!");

            _isOpened = false;
            if (_enteredCount == 0)
                _exitAwaitable.SetCompleted();
            return _exitAwaitable;
        }

        public IAwaitable ExecQueuedRequests()
        {
            Open();
            return Close();
        }

        public IAwaitable<GateToken> Enter()
        {
            if (!_isOpened)
            {
                var awaitable = GetFreeAwaitable();
                _queue.Enqueue(awaitable);
                OnWait?.Invoke();
                return awaitable;
            }
            else
            {
                _enteredCount++;
                return _completed;
            }
        }

        private ReusableAwaitable<GateToken> GetFreeAwaitable()
        {
            if (_cahce.Count > 0)
                return _cahce.Dequeue();
            else
                return new ReusableAwaitable<GateToken>();
        }

        private void Exit(ReusableAwaitable<GateToken> awaitable)
        {
            if (awaitable != null)
            {
                awaitable.Reset();
                _cahce.Enqueue(awaitable);
            }

            _enteredCount--;

            if (!_isOpened && _enteredCount <= 0)
                _exitAwaitable.SetCompleted();

            OnExit?.Invoke();
        }

        public struct GateToken : IDisposable
        {
            private AsyncGate _lock;
            private ReusableAwaitable<GateToken> _awaitable;

            internal GateToken(AsyncGate lockObj, ReusableAwaitable<GateToken> awaitable)
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
