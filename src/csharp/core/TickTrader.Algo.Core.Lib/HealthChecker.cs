using System.Threading.Tasks;
using System;
using System.Threading;

namespace TickTrader.Algo.Core.Lib
{
    public static class HealthChecker
    {
        private static readonly TimeSpan HealthCheckPeriod = TimeSpan.FromMilliseconds(1000);
        private static readonly TimeSpan BadHealthThreshold = TimeSpan.FromMilliseconds(2000);
        private static readonly TimeSpan StarvationCheckPeriod = TimeSpan.FromMilliseconds(15000);

        private static readonly object _syncObj = new object();

        private static ThreadMonitoring _instance;


        public static void Start()
        {
            lock(_syncObj)
            {
                if (_instance != null)
                    return;

                _instance = new ThreadMonitoring();
                _instance.Start();
            }
        }

        public static async Task Stop()
        {
            Task stopTask = Task.CompletedTask;

            lock (_syncObj)
            {
                if (_instance == null)
                    return;

                stopTask = _instance.Stop();
                _instance = null;
            }

            await stopTask;
        }


        private class ThreadMonitoring
        {
            private readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger(nameof(HealthChecker));

            private bool _started;
            private Task _monitorTask;


            public void Start()
            {
                _started = true;
                _monitorTask = Task.Factory.StartNew(MonitorLoop, TaskCreationOptions.LongRunning);
            }

            public Task Stop()
            {
                _started = false;
                return _monitorTask;
            }


            private void MonitorLoop()
            {
                long lastHealthCheckTicks = DateTime.UtcNow.Ticks;
                long lastStarvationCheckTicks = DateTime.UtcNow.Ticks;
                GlobalQueueItem globalQueueItem = null;

                while (_started)
                {
                    Thread.Sleep(HealthCheckPeriod);

                    var currentTicks = DateTime.UtcNow.Ticks;

                    var healthCheckTimeTicks = currentTicks - lastHealthCheckTicks;
                    lastHealthCheckTicks = currentTicks;
                    if (healthCheckTimeTicks > BadHealthThreshold.Ticks)
                        _logger.Info($"Last healthcheck too longer than expected: {TicksToMs(healthCheckTimeTicks)} ms");

                    if (globalQueueItem != null)
                    {
                        if (!_started)
                            globalQueueItem.Cancel();

                        var taskStatus = globalQueueItem.Task.Status;
                        if (taskStatus == TaskStatus.Canceled || taskStatus == TaskStatus.RanToCompletion)
                        {
                            _logger.Info($"Global queue ping back took {globalQueueItem.GetTime()} ms. Status={taskStatus}");
                            globalQueueItem = null;
                            lastStarvationCheckTicks = currentTicks;
                        }
                    }
                    else if (_started && currentTicks - lastStarvationCheckTicks > StarvationCheckPeriod.Ticks)
                    {
                        globalQueueItem = new GlobalQueueItem();
                        Task.Factory.StartNew(() => globalQueueItem.Complete(), TaskCreationOptions.PreferFairness);
                    }
                }
            }
        }


        private static double TicksToMs(long ticks) => ticks / 10_000.0;


        private class GlobalQueueItem : TaskCompletionSource<object>
        {
            private long _start, _end;


            public GlobalQueueItem()
            {
                _start = DateTime.UtcNow.Ticks;
            }

            public void Complete()
            {
                if (TrySetResult(null))
                    _end = DateTime.UtcNow.Ticks;
            }

            public void Cancel()
            {
                if (TrySetCanceled())
                    _end = DateTime.UtcNow.Ticks;
            }

            public double GetTime()
            {
                return TicksToMs(_end - _start);
            }
        }
    }
}
