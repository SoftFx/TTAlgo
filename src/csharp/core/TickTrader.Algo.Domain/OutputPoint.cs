using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace TickTrader.Algo.Domain
{
    public partial class OutputPoint
    {
        public OutputPoint(Timestamp time, int index, IMessage val)
            : this(time, index, Any.Pack(val))
        {
        }

        public OutputPoint(Timestamp time, int index, Any val)
        {
            Time = time;
            Index = index;
            Value = val;
        }


        public OutputPoint WithNewIndex(int newIndex)
        {
            return new OutputPoint(Time, newIndex, Value);
        }
    }
}
