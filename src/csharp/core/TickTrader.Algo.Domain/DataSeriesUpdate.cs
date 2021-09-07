using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace TickTrader.Algo.Domain
{
    public partial class DataSeriesUpdate
    {
        public DataSeriesUpdate(Types.Type seriesType, string seriesId, Types.UpdateAction updateAction, IMessage val)
            : this(seriesType, seriesId, updateAction, Any.Pack(val))
        {
        }

        public DataSeriesUpdate(Types.Type seriesType, string seriesId, Types.UpdateAction updateAction, Any val)
        {
            SeriesType = seriesType;
            SeriesId = seriesId;
            UpdateAction = updateAction;
            Value = val;
        }
    }
}
