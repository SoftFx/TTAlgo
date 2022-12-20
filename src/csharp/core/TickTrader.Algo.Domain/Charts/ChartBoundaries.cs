namespace TickTrader.Algo.Domain
{
    public interface IChartBoundaries
    {
        int? BarsCount { get; }
    }


    public class ChartBoundaries : IChartBoundaries
    {
        public int? BarsCount { get; set; }


        public override string ToString()
        {
            return $"Rolling windows: {BarsCount} bars";
        }
    }
}
