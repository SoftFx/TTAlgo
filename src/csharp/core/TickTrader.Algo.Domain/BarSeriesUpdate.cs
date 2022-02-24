namespace TickTrader.Algo.Domain
{
    public partial class BarSeriesUpdate
    {
        public BarSeriesUpdate(Types.Type seriesType, string seriesId, DataSeriesUpdate.Types.Action updateAction, BarData bar)
        {
            SeriesType = seriesType;
            SeriesId = seriesId;
            UpdateAction = updateAction;
            Bar = bar;
        }
    }
}
