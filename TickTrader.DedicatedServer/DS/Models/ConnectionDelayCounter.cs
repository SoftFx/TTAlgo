using System;

namespace TickTrader.DedicatedServer.DS.Models
{
    internal interface IDelayCounter
    {
        TimeSpan Value { get; }
        TimeSpan Next();
        void Reset();
    }

    internal class ConnectionDelayCounter : IDelayCounter
    {
        private TimeSpan _minDelay;
        private TimeSpan _maxDelay;
        private TimeSpan? _currentDelay;
        private object _syncOnj = new object();

        public ConnectionDelayCounter(TimeSpan minDelay, TimeSpan maxDelay)
        {
            if (minDelay > maxDelay)
                throw new ArgumentException("maxDelay should be more then minDelay");

            _maxDelay = maxDelay;
            _minDelay = minDelay;
            _currentDelay = null;
        }

        public TimeSpan Value
        {
            get
            {
                lock (_syncOnj)
                    return _currentDelay ?? _minDelay;
            }
        }

        public TimeSpan Next()
        {
            lock (_syncOnj)
            {
                if (!_currentDelay.HasValue)
                    _currentDelay = _minDelay;
                else if (_currentDelay.Value == _maxDelay)
                    _currentDelay = _maxDelay;
                else
                {
                    var nextDelay = TimeSpan.FromMilliseconds(_currentDelay.Value.TotalMilliseconds * 2);

                    if (_maxDelay <= nextDelay)
                        _currentDelay = _maxDelay;
                    else
                        _currentDelay = nextDelay;
                }

                return _currentDelay.Value;
            }
        }

        public void Reset()
        {
            lock (_syncOnj)
                _currentDelay = null;
        }
    }

    
}
