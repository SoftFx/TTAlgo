using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal class TimerFixture : Api.ITimerApi, IExecutorFixture
    {
        private IFixtureContext _context;
        private List<TimerProxy> _timers = new List<TimerProxy>();
        private bool _isStarted;

        public TimerFixture(IFixtureContext context)
        {
            _context = context;
        }

        public void Start()
        {
            lock (_timers)
            {
                _context.Builder.TimerApi = this;
                _isStarted = true;
            }
        }

        public void Restart()
        {
        }

        public void Stop()
        {
            lock (_timers)
            {
                _isStarted = false;

                foreach (var timer in _timers)
                {
                    timer.Disposed -= Proxy_Disposed;
                    timer.Dispose();
                }

                _timers.Clear();
            }
        }

        #region ITimerApi implementation

        DateTime ITimerApi.Now => DateTime.Now;
        DateTime ITimerApi.UtcNow => DateTime.UtcNow;

        Timer ITimerApi.CreateTimer(TimeSpan period, Action<Timer> callback)
        {
            lock (_timers)
            {
                if (!_isStarted)
                    throw new InvalidOperationException("Timer API is disabled!");

                var proxy = new TimerProxy(period, _context, callback);

                _timers.Add(proxy);
                proxy.Disposed += Proxy_Disposed;

                return proxy;
            }
        }

        Task ITimerApi.Delay(TimeSpan period)
        {
            return Task.Delay(period);
        }

        #endregion

        private void Proxy_Disposed(TimerProxy timer)
        {
            lock (_timers)
            {
                _timers.Remove(timer);
                timer.Disposed -= Proxy_Disposed;
            }
        }

        public void Dispose()
        {
            Stop();
        }

        private class TimerProxy : Timer
        {
            private System.Threading.Timer _timer;
            private IFixtureContext _context;
            private Action<Timer> _callback;

            public TimerProxy(TimeSpan period, IFixtureContext context, Action<Timer> callback)
            {
                _context = context;
                _timer = new System.Threading.Timer(OnTick, null, period, period);
                _callback = callback;
            }

            private void InvokeCallback(PluginBuilder builder)
            {
                builder.InvokeAsyncAction(() => _callback(this));
            }

            private void OnTick(object state)
            {
                _context.EnqueueCustomInvoke(InvokeCallback);
            }

            public void Change(int periodMs)
            {
                _timer.Change(periodMs, periodMs);
            }

            public void Change(TimeSpan period)
            {
                _timer.Change(period, period);
            }

            public void Dispose()
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                    Disposed?.Invoke(this);
                }
            }

            public event Action<TimerProxy> Disposed;
        }
    }
}
