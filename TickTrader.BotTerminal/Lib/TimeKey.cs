using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    public struct TimeKey : IComparable, IComparable<TimeKey>
    {
        public TimeKey(DateTime time, int shift)
        {
            Timestamp = time;
            Shift = shift;
        }

        public TimeKey(Timestamp time)
        {
            Timestamp = time.ToDateTime();
            Shift = time.Nanos % 100; // DateTime tick is 100 ns
        }

        public DateTime Timestamp { get; }
        public int Shift { get; }

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
            return $"{Timestamp.Ticks}.{Shift}";
        }
    }
}
