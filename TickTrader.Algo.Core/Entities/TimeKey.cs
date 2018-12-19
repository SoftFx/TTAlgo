using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public struct TimeKey : IComparable, IComparable<TimeKey>
    {
        public TimeKey(DateTime timestamp, uint shift)
        {
            Timestamp = timestamp;
            Shift = shift;
        }

        public DateTime Timestamp { get; }
        public uint Shift { get; }

        public TimeKey ToLocalTime()
        {
            return new TimeKey(Timestamp.ToLocalTime(), Shift);
        }

        public int CompareTo(object obj)
        {
            return CompareTo((TimeKey)obj);
        }

        public int CompareTo(TimeKey other)
        {
            var timeCmp = Timestamp.CompareTo(other.Timestamp);
            if (timeCmp != 0)
                return timeCmp;
            return Shift.CompareTo(other.Shift);
        }

        public override bool Equals(object obj)
        {
            if (obj is TimeKey)
            {
                var other = (TimeKey)obj;
                return other.Timestamp == Timestamp && other.Shift == Shift;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ Timestamp.GetHashCode();
                hash = (hash * 16777619) ^ Shift.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return Timestamp.ToShortDateString() + " [" + Shift + "]";
        }
    }

    public class TimeKeyGenerator
    {
        private DateTime _time;
        private uint _shift;

        public void Reset()
        {
            _time = DateTime.MinValue;
            _shift = 0;
        }

        public TimeKey NextKey(DateTime timestamp)
        {
            var cmp = timestamp.CompareTo(_time);

            if (cmp > 0)
            {
                _time = timestamp;
                _shift = 0;
            }
            else if (cmp < 0)
                throw new ArgumentException("Invalid time sequence: Provided datetime is less than last recorded.");

            return new TimeKey(_time, _shift++);
        }
    }
}
