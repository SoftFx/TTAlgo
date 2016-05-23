using System.Collections.Generic;
using SoftFx.FxCalendar.Calendar.FxStreet;
using SoftFx.FxCalendar.Filters;
using SoftFx.FxCalendar.Providers;
using SoftFx.FxCalendar.Storage;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.Other.News
{
    [Indicator(Category = "Other", DisplayName = "Other/News")]
    public class News : Indicator
    {
        private string _firstCurrency, _secondCurrency, _additionalCurrency;
        private List<FxStreetProvider> _providers;
        private List<DataSeries> _providerOutputs;

        [Parameter(DefaultValue = "EURUSD", DisplayName = "Symbol")]
        public string SymbolCode { get; set; }

        [Parameter(DefaultValue = "", DisplayName = "Additional Currency")]
        public string AdditionalCurrency { get; set; }

        [Input]
        public BarSeries Bars { get; set; }

        [Output(DisplayName = "First Currency Impact", DefaultColor = Colors.Green)]
        public DataSeries FirstCurrencyImpact { get; set; }

        [Output(DisplayName = "Second Currency Impact", DefaultColor = Colors.Red)]
        public DataSeries SecondCurrencyImpact { get; set; }

        [Output(DisplayName = "Additional Currency Impact", DefaultColor = Colors.SteelBlue)]
        public DataSeries AdditionalCurrencyImpact { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public News() { }

        public News(BarSeries bars)
        {
            Bars = bars;

            InitializeIndicator();
        }

        private void InitializeIndicator()
        {
            _firstCurrency = SymbolCode.Substring(0, 3);
            _secondCurrency = SymbolCode.Substring(3, 3);
            _additionalCurrency = null;

            _providers = new List<FxStreetProvider>();
            _providerOutputs = new List<DataSeries>();

            _providers.Add(new FxStreetProvider(new FxStreetCalendar(new FxStreetFilter()),
                new FxStreetStorage("NewsCache", _firstCurrency), ImpactLevel.None));
            _providers.Add(new FxStreetProvider(new FxStreetCalendar(new FxStreetFilter()),
                new FxStreetStorage("NewsCache", _secondCurrency), ImpactLevel.None));

            _providerOutputs.Add(FirstCurrencyImpact);
            _providerOutputs.Add(SecondCurrencyImpact);

            if (AdditionalCurrency.Length == 3)
            {
                _additionalCurrency = AdditionalCurrency.Substring(0, 3);
                _providers.Add(new FxStreetProvider(new FxStreetCalendar(new FxStreetFilter()),
                new FxStreetStorage("NewsCache", _additionalCurrency), ImpactLevel.None));
                _providerOutputs.Add(AdditionalCurrencyImpact);
            }
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = 0;

            for (var i = 0; i < _providers.Count; i++)
            {
                _providerOutputs[i][pos] = GetImpact(pos, _providers[i]);
            }
        }

        private double GetImpact(int pos, FxStreetProvider provider)
        {
            var res = 0.0;

            foreach (var newsModel in provider.GetNews(Bars[pos].OpenTime, Bars[pos].CloseTime))
            {
                res = res > (int) newsModel.Impact ? res : (int) newsModel.Impact;
            }

            return res;
        }
    }
}