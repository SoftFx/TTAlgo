using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Machinarium.ActorModel
{
    public class ExecRequest : TaskCompletionSource<IDisposable>, IDisposable
    {
        private Actor _actor;

        internal ExecRequest(Actor actor)
        {
            _actor = actor;
        }

        internal bool TryStart()
        {
            return TrySetResult(this);
        }

        public void Dispose()
        {
            _actor.Release(this);
        }
    }

    public class Actor
    {
        private int _requestCount;
        private bool _isRunning;
        private ExecRequest _currentRequest;
        private List<ActorBlock> _inputs = new List<ActorBlock>();

        internal object LockObj => _inputs;

        internal void AddInput(ActorBlock block)
        {
            lock (LockObj)
            {
                _inputs.Add(block);
                _requestCount += block.RequestCount;
                if (!_isRunning && _requestCount > 0)
                    StartNext();
            }
        }

        internal void OnNewRequest()
        {
            _requestCount++;
            if (!_isRunning)
                StartNext();
        }

        internal void Release(ExecRequest request)
        {
            lock (LockObj)
            {
                if (_currentRequest == request)
                {
                    _currentRequest = null;
                    if (_requestCount > 0)
                        StartNext();
                    else
                        _isRunning = false;
                }
            }
        }

        private ExecRequest Dequeue()
        {
            foreach (var i in _inputs)
            {
                ExecRequest request = i.Take();
                if (request != null)
                {
                    _requestCount--;
                    return request;
                }
            }

            return null;
        }

        private void StartNext()
        {
            _isRunning = true;

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    lock (LockObj)
                    {
                        _currentRequest = Dequeue();
                        if (_currentRequest == null)
                        {
                            _isRunning = false;
                            return;
                        }
                    }

                    if (_currentRequest.TryStart())
                        return;
                }
            });
        }

        internal ExecRequest CreateRequest()
        {
            return new ExecRequest(this);
        }
    }

    public abstract class ActorBlock
    {
        public ActorBlock(Actor actor)
        {
            Actor = actor ?? new Actor();
            Actor.AddInput(this);
        }

        internal abstract int RequestCount { get; }
        public Actor Actor { get; }

        public Task<IDisposable> GetLock()
        {
            return GetLock(CancellationToken.None);
        }

        public Task<IDisposable> GetLock(CancellationToken cancelToken)
        {
            var request = Actor.CreateRequest();

            if (cancelToken.CanBeCanceled)
                cancelToken.Register(() => request.TrySetCanceled());

            lock (Actor.LockObj) Enqueue(request);

            return request.Task;
        }

        public Task InvokeExlusive(Action action)
        {
            return InvokeExlusive(action, CancellationToken.None);
        }

        public Task InvokeExlusive(Action action, CancellationToken cancelToken)
        {
            Action<Task<IDisposable>> invoker = t =>
            {
                try
                {
                    action();
                }
                finally
                {
                    t.Result.Dispose();
                }
            };

            return GetLock(cancelToken).ContinueWith(invoker, TaskContinuationOptions.ExecuteSynchronously);
        }

        public Task<T> InvokeExlusive<T>(Func<T> action)
        {
            return InvokeExlusive<T>(action, CancellationToken.None);
        }

        public Task<T> InvokeExlusive<T>(Func<T> action, CancellationToken cancelToken)
        {
            Func<Task<IDisposable>, T> invoker = t =>
            {
                try
                {
                    return action();
                }
                finally
                {
                    t.Result.Dispose();
                }
            };

            return GetLock(cancelToken).ContinueWith(invoker, TaskContinuationOptions.ExecuteSynchronously);
        }

        internal ExecRequest Take()
        {
            return Dequeue();
        }

        protected abstract void Enqueue(ExecRequest request);
        protected abstract ExecRequest Dequeue();
    }

    public class QueueBlock : ActorBlock
    {
        private Queue<ExecRequest> _waiters = new Queue<ExecRequest>();

        public QueueBlock(Actor actor = null) : base(actor)
        {
        }

        internal override int RequestCount => _waiters.Count;

        protected override ExecRequest Dequeue()
        {
            if (_waiters.Count > 0)
                return _waiters.Dequeue();
            return null;
        }

        protected override void Enqueue(ExecRequest request)
        {
            _waiters.Enqueue(request);
            Actor.OnNewRequest();
        }
    }

    public class PushOutBlock : ActorBlock
    {
        private ExecRequest _current;

        internal override int RequestCount => _current == null ? 0 : 1;

        public PushOutBlock(Actor actor = null) : base(actor)
        {
        }

        protected override ExecRequest Dequeue()
        {
            var copy = _current;
            _current = null;
            return copy;
        }

        protected override void Enqueue(ExecRequest request)
        {
            if (_current != null)
            {
                _current.TrySetCanceled();
                _current = request;
            }
            else
            {
                _current = request;
                Actor.OnNewRequest();
            }
        }
    }
}
