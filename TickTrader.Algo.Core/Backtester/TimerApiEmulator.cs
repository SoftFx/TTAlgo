using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal class TimerApiEmulator : ITimerApi, IExecutorFixture
    {
        private InvokeEmulator _scheduler;
        private IFixtureContext _context;

        public TimerApiEmulator(IFixtureContext context, InvokeEmulator scheduler)
        {
            _scheduler = scheduler;
            _context = context;
        }

        public void Start()
        {
            _context.Builder.TimerApi = this;
        }

        public void Stop()
        {
        }

        public void Restart()
        {
        }

        public void Dispose()
        {
        }

        #region ITimerApi

        public DateTime Now => _scheduler.SafeVirtualTimePoint;
        public DateTime UtcNow => DateTime.SpecifyKind(Now, DateTimeKind.Utc);

        public Timer CreateTimer(TimeSpan period, Action<Timer> callback)
        {
            return new TimerEmulator(this, period, callback);
        }

        public Task Delay(TimeSpan period)
        {
            return _scheduler.EmulateAsyncDelay(period, false);
        }

        #endregion

        private class TimerEmulator : Api.Timer
        {
            private TimerApiEmulator _parent;
            private Action<Timer> _callback;
            private TimeSpan _period;
            private bool _isStopped;

            public TimerEmulator(TimerApiEmulator parent, TimeSpan period, Action<Timer> callback)
            {
                _parent = parent;
                _callback = callback;
                TimeLoop();
            }

            private async void TimeLoop()
            {
                await _parent._scheduler.EmulateAsyncDelay(_period, false);
                if (_isStopped)
                    return;
                _parent._context.Builder.InvokeAsyncAction(() => _callback(this));
            }

            public void Change(int periodMs)
            {
                _period = TimeSpan.FromMilliseconds(periodMs);
            }

            public void Change(TimeSpan period)
            {
                _period = period;
            }

            public void Dispose()
            {
                _isStopped = true;
            }
        }
    }
}
