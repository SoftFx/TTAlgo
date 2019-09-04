using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Lib
{
    public class BunchingBlock<T>
    {
        private readonly object _sycObj = new object();
        private readonly Queue<List<T>> _pages = new Queue<List<T>>();
        private readonly int _maxQueueSize;
        private readonly int _maxPageSize;
        private readonly Action<List<T>> _handler;
        private bool _isProcessing;
        private List<T> _lastPage;
        private readonly TaskCompletionSource<object> _completionSrc = new TaskCompletionSource<object>();
        private bool _isCompleted;

        public BunchingBlock(Action<List<T>> handler, int maxPageSize, int maxQueueSize = -1)
        {
            _maxPageSize = maxPageSize;
            _maxQueueSize = maxQueueSize;
            _handler = handler;
        }

        public int Count { get; private set; }

        public void Enqueue(T item)
        {
            lock (_sycObj)
            {
                if (_maxQueueSize > 0)
                {
                    while (Count >= _maxQueueSize && !_isCompleted)
                        Monitor.Wait(_sycObj);
                }

                if (_isCompleted)
                    throw new InvalidOperationException("Block is completed!");

                if (_lastPage == null || _lastPage.Count >= _maxPageSize)
                {
                    var newPage = new List<T>();
                    newPage.Add(item);
                    _lastPage = newPage;
                    _pages.Enqueue(newPage);
                }
                else
                    _lastPage.Add(item);

                Count++;

                ScheduleProcessing();
            }
        }

        public Task Completion => _completionSrc.Task;

        public void Complete(bool dropQueue = false)
        {
            lock (_sycObj)
            {
                _isCompleted = true;
                if (dropQueue)
                    _pages.Clear();
                if (_pages.Count == 0 && !_isProcessing)
                    _completionSrc.TrySetResult(this);
            }
        }

        private void ScheduleProcessing()
        {
            if (_isProcessing)
                return;

            Task.Factory.StartNew(ProcessItems);
            _isProcessing = true;
        }

        private void ProcessItems()
        {
            const int maxItemsPerTask = 1000;

            int processedItemsCount = 0;

            while (true)
            {
                List<T> nextPage;

                lock (_sycObj)
                {
                    if (_pages.Count == 0)
                    {
                        _isProcessing = false;

                        if (_isCompleted)
                            _completionSrc.TrySetResult(this);

                        return;
                    }

                    if (processedItemsCount >= maxItemsPerTask)
                    {
                        Task.Factory.StartNew(ProcessItems);
                        return;
                    }

                    nextPage = _pages.Dequeue();

                    if (_pages.Count == 0)
                        _lastPage = null;

                    Count -= nextPage.Count;

                    if (_maxQueueSize > 0)
                        Monitor.Pulse(_sycObj);
                }

                _handler(nextPage);

                processedItemsCount += nextPage.Count;
            }
        }
    }
}
