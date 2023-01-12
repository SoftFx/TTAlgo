namespace TickTrader.Algo.Domain
{
    public class OutputInfo
    {
        public string PluginId { get; set; }

        public string SeriesId { get; set; }

        public OutputDescriptor Descriptor { get; set; }

        public IOutputConfig Config { get; set; }
    }
}
