using TickTrader.Algo.Server;

namespace TickTrader.BotTerminal
{
    internal class IndicatorViewModel
    {
        private readonly ChartHostProxy _chart;


        public PluginOutputModel Model { get; }

        public string DisplayName => Model.Id;


        public IndicatorViewModel(ChartHostProxy chart, PluginOutputModel indicator)
        {
            _chart = chart;
            Model = indicator;
        }


        public void Close()
        {
            _ = _chart.RemoveIndicator(Model.Id);
        }
    }
}
