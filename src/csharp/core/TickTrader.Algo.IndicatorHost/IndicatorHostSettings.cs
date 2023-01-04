using TickTrader.Algo.Server;

namespace TickTrader.Algo.IndicatorHost
{
    public class IndicatorHostSettings
    {
        public string DataFolder { get; set; }

        public AlgoHostSettings HostSettings { get; set; } = new AlgoHostSettings();
    }
}
