using Google.Protobuf.WellKnownTypes;
using System;

namespace TickTrader.Algo.Core
{
    // Normally C# DateTime.Now has ~1ms resoultion which leads to ~1,000,000 of unused values in Timestamp.Nanos
    // We are using those values to differ and preserve sort order of events which occur during 1ms interval
    // This should be more than enough during normal execution
    // As for backtester it's virtual time is frozen during code execution and is changed by integer number of ms
    // So 1ms can last indefinetely and code can generate more than 1,000,000 events during several consequtive ms and break event sort order
    // But this case is highly artificial and lies beyond any reasonable usage scope we could imagine
    public class TimeKeyGenerator
    {
        private Timestamp _timestamp;
        private long _lastTicks;

        public void Reset()
        {
            _timestamp = new Timestamp(); // 1970-01-01
            _lastTicks = 0;
        }

        public Timestamp NextKey(DateTime time)
        {
            var ticks = time.Ticks;
            var diff = ticks - _lastTicks;

            if (diff > 0)
            {
                _lastTicks = ticks;
                _timestamp = time.ToUniversalTime().ToTimestamp();
            }
            else if (diff == 0)
            {
                var nanos = _timestamp.Nanos + 1;
                if (nanos > 999_999_999)
                {
                    _timestamp.Seconds++;
                    nanos = 0;
                }
                _timestamp.Nanos = nanos;
            }
            else
                throw new ArgumentException("Invalid time sequence: Provided datetime is less than last recorded.");

            return new Timestamp(_timestamp);
        }
    }
}
