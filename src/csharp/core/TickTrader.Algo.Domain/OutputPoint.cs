using Google.Protobuf;

namespace TickTrader.Algo.Domain
{
    public readonly struct OutputPoint
    {
        // X coordinate
        public long Time { get; }

        // Y coordinate
        public double Value { get; }

        public object Metadata { get; }


        public OutputPoint(long time, double value)
        {
            Time = time;
            Value = value;
            Metadata = null;
        }

        public OutputPoint(long time, double value, MarkerInfo marker)
        {
            Time = time;
            Value = value;
            Metadata = marker;
        }
    }


    public partial class OutputPointWire
    {
        public OutputPointWire(OutputPoint point)
        {
            Time = point.Time;
            Value = point.Value;
            Type = Types.Type.Double;
            switch(point.Metadata)
            {
                case MarkerInfo marker:
                    Type = Types.Type.Marker;
                    Metadata = marker.ToByteString();
                    break;
            }
        }


        public OutputPoint Unpack()
        {
            switch (Type)
            {
                case Types.Type.Double: break;
                case Types.Type.Marker: return new OutputPoint(Time, Value, (MarkerInfo)MarkerInfo.Descriptor.Parser.ParseFrom(Metadata));
            }

            return new OutputPoint(Time, Value);
        }
    }
}
