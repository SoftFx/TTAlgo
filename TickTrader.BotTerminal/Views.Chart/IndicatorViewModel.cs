namespace TickTrader.BotTerminal
{
    internal class IndicatorViewModel
    {
        private ChartModelBase _chart;


        public IndicatorModel Model { get; }

        public string DisplayName => Model.InstanceId;


        public IndicatorViewModel(ChartModelBase chart, IndicatorModel indicator)
        {
            _chart = chart;
            Model = indicator;
        }


        public void Close()
        {
            _chart.RemoveIndicator(Model);
        }
    }
}
