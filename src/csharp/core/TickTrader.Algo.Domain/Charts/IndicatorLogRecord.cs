namespace TickTrader.Algo.Domain
{
    public class IndicatorLogRecord
    {
        public string PluginId { get; }

        public int ChartId { get; } // needed to allow chart name override

        public string ChartName { get; }

        public PluginLogRecord LogRecord { get; }


        public IndicatorLogRecord(string pluginId, int chartId, string chartName, PluginLogRecord logRecord)
        {
            PluginId = pluginId;
            ChartId = chartId;
            ChartName = chartName;
            LogRecord = logRecord;
        }
    }
}
