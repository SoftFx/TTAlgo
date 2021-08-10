using System.Collections.Generic;

namespace TickTrader.Algo.Async.Actors
{
    /// <summary>
    /// Not thread-safe. Should be used only in actors
    /// </summary>
    public class ActorLock : IAsyncTokenOwner
    {
        private readonly IMsgDispatcher _msgDispatcher;
        private readonly Queue<ReusableAsyncToken> _queue = new Queue<ReusableAsyncToken>();
        private readonly Queue<ReusableAsyncToken> _cache = new Queue<ReusableAsyncToken>();
        private readonly ReusableAsyncToken _completed;

        private bool _isTaken;


        public string TakenBy { get; private set; }


        // use Actor.CreateLock()
        internal ActorLock(IMsgDispatcher msgDispatcher)
        {
            _msgDispatcher = msgDispatcher;

            _completed = ReusableAsyncToken.CreateCompleted(this);
        }


        public IAsyncToken GetLock(string lockerName = null)
        {
            if (_isTaken)
            {
                var token = GetFreeToken();
                token.WaiterName = lockerName;
                _queue.Enqueue(token);
                return token;
            }
            else
            {
                _isTaken = true;
                TakenBy = lockerName;
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

            if (_queue.Count > 0)
            {
                var next = _queue.Dequeue();
                TakenBy = next.WaiterName;

                // despite the fact that we should be in actor context already,
                // we have to pass continuations through actor mailbox
                _msgDispatcher.PostMessage(token);
            }
            else
            {
                TakenBy = null;
                _isTaken = false;
            }
        }
    }
}
