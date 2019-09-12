using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public struct OutputPoint<T>
    {
        public OutputPoint(DateTime? time, int index, T val)
        {
            this.TimeCoordinate = time;
            this.Index = index;
            this.Value = val;
        }

        public DateTime? TimeCoordinate { get; }
        public T Value { get; }
        public int Index { get; }

        public OutputPoint<T> ChangeIndex(int newIndex)
        {
            return new OutputPoint<T>(TimeCoordinate, newIndex, Value);
        }
    }
}
