using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ActorSharp
{
    public class PoolContext : SynchronizationContext
    {
        private Queue<CallbackItem> _queue = new Queue<CallbackItem>();
        private List<CallbackItem> _page = new List<CallbackItem>();
        private int _pageMaxSize = 10;
        private bool _isExecuting;

        public PoolContext(int maxMessagePerTask = 10, string poolName = null)
        {
            Name = poolName;

            if (maxMessagePerTask < 1)
                throw new ArgumentException("maxMessagePerTask");

            _pageMaxSize = maxMessagePerTask;
        }

        public string Name { get; private set; }

        public override void Post(SendOrPostCallback d, object state)
        {
            Enqueue(new CallbackItem(d, state));
        }

        private void Enqueue(CallbackItem item)
        {
            lock (_queue)   
            {
                _queue.Enqueue(item);
                if (!_isExecuting)
                {
                    _isExecuting = true;
                    ScheduleNextPage();
                }
            }
        }

        private void ScheduleNextPage()
        {
            if (_queue.Count > 0)
            {
                DequeueNextPage();
                Task.Factory.StartNew(ProcessItems, _page);
                //ThreadPool.QueueUserWorkItem(ProcessItems, _page);
            }
            else
                _isExecuting = false;
        }

        private void DequeueNextPage()
        {
            _page.Clear();
            var pageSize = Math.Min(_pageMaxSize, _queue.Count);
            for (int i = 0; i < pageSize; i++)
                _page.Add(_queue.Dequeue());
        }

        private void ProcessItems(object state)
        {
            try
            {
                SetSynchronizationContext(this);

                foreach (var item in (List<CallbackItem>)state)
                    item.Callback(item.State);
            }
            catch (Exception ex)
            {
                Actor.OnActorFailed(ex);
                //Environment.FailFast("Uncaught exception in actor: " + ex.Message, ex);
                throw;
            }
            finally
            {
                SetSynchronizationContext(null);
            }

            lock (_queue) ScheduleNextPage();
        }

        private void InvokeItem(object state)
        {
            try
            {
                var item = (Tuple<SendOrPostCallback, object>)state;

                SetSynchronizationContext(this);
                item.Item1(item.Item2);
            }
            catch (Exception ex)
            {
                Environment.FailFast("Uncaught exception in actor!", ex);
            }
            finally
            {
                SetSynchronizationContext(null);
            }
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            throw new NotImplementedException("Should not Send() in Actor famework!");
        }

        //public override int Wait(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
        //{
        //    throw new NotImplementedException("Should not Wait() in Actor famework!");
        //}

        private struct CallbackItem
        {
            public CallbackItem(SendOrPostCallback callback, object state)
            {
                Callback = callback;
                State = state;
            }

            public SendOrPostCallback Callback { get; private set; }
            public object State { get; private set; }
        }
    }
}
