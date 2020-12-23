using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.BotAgent.Infrastructure
{
    internal class ThreadPoolManager
    {
        private static readonly ILogger _logger = LogManager.GetLogger(nameof(ThreadPoolManager));

        private int _processorCnt;
        private int _lastMinThreads;
        private bool _started;
        private Task _monitorTask;

        public void Start(int botsCnt)
        {
            _processorCnt = Environment.ProcessorCount;
            _lastMinThreads = 0;
            OnNewBotsCnt(botsCnt);
            _started = true;

            _monitorTask = Task.Factory.StartNew(MonitorStarvation, TaskCreationOptions.LongRunning);
        }

        public Task Stop()
        {
            _started = false;
            return _monitorTask;
        }


        public void OnNewBotsCnt(int botsCnt)
        {
            SetMinThreads(2 * botsCnt);
        }


        private void SetMinThreads(int required)
        {
            var newMinThreads = ((required % _processorCnt) + 1) * _processorCnt;
            if (_lastMinThreads != newMinThreads)
            {
                _logger.Debug($"ThreadPool min threads changed to {newMinThreads}");
                ThreadPool.SetMinThreads(newMinThreads, newMinThreads);
            }
        }


        private void MonitorStarvation()
        {
            long cnt = 0;
            GlobalQueueItem globalQueueItem = null;
            while (_started)
            {
                Thread.Sleep(1000);
                cnt++;
                if (globalQueueItem != null)
                {
                    if (!_started)
                        globalQueueItem.Cancel();

                    var taskStatus = globalQueueItem.Task.Status;
                    if (taskStatus == TaskStatus.Canceled || taskStatus == TaskStatus.RanToCompletion)
                    {
                        _logger.Info($"Global queue ping back took {globalQueueItem.GetTime()} ms. Status={taskStatus}");
                        globalQueueItem = null;
                        cnt = 0;
                    }
                }
                else if (_started && cnt >= 60)
                {
                    globalQueueItem = new GlobalQueueItem();
                    Task.Factory.StartNew(() => globalQueueItem.Complete(), TaskCreationOptions.PreferFairness);
                }
            }
        }


        private class GlobalQueueItem : TaskCompletionSource<object>
        {
            private long _start;
            private long _end;

            public GlobalQueueItem()
            {
                _start = DateTime.Now.Ticks;
            }

            public void Complete()
            {
                if (TrySetResult(null))
                    _end = DateTime.Now.Ticks;
            }

            public void Cancel()
            {
                if (TrySetCanceled())
                    _end = DateTime.Now.Ticks;
            }

            public double GetTime()
            {
                return (_end - _start) / 10_000.0;
            }
        }
    }
}
