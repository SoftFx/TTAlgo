using System;
using System.Collections.Generic;

namespace TickTrader.Algo.Async.Actors
{
    public class ActorGate : IAsyncTokenOwner
    {
        private readonly IMsgDispatcher _msgDispatcher;
        private readonly Queue<ReusableAsyncToken> _queue = new Queue<ReusableAsyncToken>();
        private readonly Queue<ReusableAsyncToken> _cache = new Queue<ReusableAsyncToken>();
        private readonly ReusableAsyncToken _completed;
        private readonly ReusableAsyncToken _exitToken;

        private int _enteredCount;
        private bool _isOpened;


        public int WatingCount => _queue.Count;

        public int EnteredCount => _enteredCount;


        public event Action OnWait;
        public event Action OnExit;


        // use Actor.CreateGate()
        internal ActorGate(IMsgDispatcher msgDispatcher)
        {
            _msgDispatcher = msgDispatcher;

            _completed = ReusableAsyncToken.CreateCompleted(this);
            _exitToken = new ReusableAsyncToken(null);
        }


        public void Open()
        {
            if (_isOpened)
                throw new InvalidOperationException("Gate is already opened!");

            if (_enteredCount > 0)
                throw new InvalidOperationException("Gate is not yet closed!");

            _isOpened = true;
            _exitToken.Reset();

            while (_queue.Count > 0)
            {
                var token = _queue.Dequeue();
                _msgDispatcher.PostMessage(token);
                _enteredCount++;
            }
        }

        public IAsyncToken Close()
        {
            if (!_isOpened)
                throw new InvalidOperationException("Gate is already closed!");

            _isOpened = false;
            if (_enteredCount == 0)
                _exitToken.SetCompleted();
            return _exitToken;
        }

        public IAsyncToken ExecQueuedRequests()
        {
            Open();
            return Close();
        }

        public IAsyncToken Enter()
        {
            if (!_isOpened)
            {
                var token = GetFreeToken();
                _queue.Enqueue(token);
                OnWait?.Invoke();
                return token;
            }
            else
            {
                _enteredCount++;
                return _completed;
            }
        }


        private ReusableAsyncToken GetFreeToken() => _cache.Count > 0 ? _cache.Dequeue() : new ReusableAsyncToken(this);


        void IAsyncTokenOwner.Return(ReusableAsyncToken token)
        {
            if (token != null)
            {
                token.Reset();
                _cache.Enqueue(token);
            }

            _enteredCount--;

            if (!_isOpened && _enteredCount <= 0)
                _exitToken.SetCompleted();

            if (_enteredCount <= 0)
                OnExit?.Invoke();
        }
    }
}
