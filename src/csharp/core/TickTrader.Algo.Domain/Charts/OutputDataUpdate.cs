namespace TickTrader.Algo.Domain
{
    public class OutputDataUpdate
    {
        public string PluginId { get; set; }

        public OutputSeriesUpdate Update { get; set; }
    }
}
