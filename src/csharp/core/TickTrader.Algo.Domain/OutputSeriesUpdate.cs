using System.Collections.Generic;

namespace TickTrader.Algo.Domain
{
    public partial class OutputSeriesUpdate
    {
        public OutputSeriesUpdate(string seriesId, DataSeriesUpdate.Types.Action updateAction, OutputPointWire point)
            : this(seriesId, updateAction)
        {
            Points.Add(point);
        }

        public OutputSeriesUpdate(string seriesId, DataSeriesUpdate.Types.Action updateAction, IEnumerable<OutputPointWire> points)
            : this(seriesId, updateAction)
        {
            Points.AddRange(points);
        }

        public OutputSeriesUpdate(string seriesId, DataSeriesUpdate.Types.Action updateAction)
        {
            SeriesId = seriesId;
            UpdateAction = updateAction;
        }
    }
}
