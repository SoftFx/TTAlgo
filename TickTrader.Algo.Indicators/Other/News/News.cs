using System.Collections.Generic;
using SoftFx.FxCalendar.Calendar.FxStreet;
using SoftFx.FxCalendar.Filters;
using SoftFx.FxCalendar.Providers;
using SoftFx.FxCalendar.Storage;
using TickTrader.Algo.Api;
using SoftFx.FxCalendar.Models;
using System.Text;
using System.Linq;

namespace TickTrader.Algo.Indicators.Other.News
{
    [Indicator(Category = "Other", DisplayName = "Other/News")]
    public class News : Indicator
    {
        private string _firstCurrency, _secondCurrency, _additionalCurrency;
        private List<FxStreetProvider> _providers;
        private List<DataSeries> _providerOutputs;
        private StringBuilder markerTextBuilder = new StringBuilder();
        private List<FxStreetNewsModel> markerNewsList = new List<FxStreetNewsModel>();

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

        [Output(DisplayName = "News Markers", DefaultColor = Colors.Gray)]
        public DataSeries<Marker> Markers{ get; set; }

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
            for (var i = 0; i < _providers.Count; i++)
            {
                _providerOutputs[i][0] = GetImpact(0, _providers[i]);
            }

            if (_additionalCurrency == null)
            {
                AdditionalCurrencyImpact[0] = double.NaN;
            }

            var averageImpact = _providerOutputs.Average(o => o[0]);

            DrawNewsMarker(averageImpact);
        }

        private IEnumerable<FxStreetNewsModel> GetCurrentNews(FxStreetProvider provider)
        {
            var from = Bars.Count > 1 ? Bars[1].CloseTime : Bars[0].OpenTime;
            var to = Bars[0].CloseTime;

            return provider.GetNews(from, to);
        }

        private void DrawNewsMarker(double level)
        {
            markerTextBuilder.Clear();
            markerNewsList.Clear();

            foreach (var provider in _providers)
                markerNewsList.AddRange(GetCurrentNews(provider));

            if (markerNewsList.Count > 0)
            {
                foreach (var n in markerNewsList)
                    markerTextBuilder.Append(n.Impact).Append("  ").Append(n.Event).Append(" ").Append(n.Actual).AppendLine();

                var maxImpact = markerNewsList.Max(n => n.Impact);

                Markers[0].Y = level;
                Markers[0].Icon = MarkerIcons.Diamond;
                Markers[0].DisplayText = markerTextBuilder.ToString();

                if (maxImpact == ImpactLevel.High)
                    Markers[0].Color = Colors.OrangeRed;
                else if (maxImpact == ImpactLevel.Medium)
                    Markers[0].Color = Colors.LightGreen;
            }
        }

        private double GetImpact(int pos, FxStreetProvider provider)
        {
            var res = 0.0;

            foreach (var newsModel in provider.GetNews(Bars[pos].OpenTime, Bars[pos].CloseTime))
            {
                res = res > (int)newsModel.Impact + 1 ? res : (int)newsModel.Impact + 1;
            }

            if (provider.Storage.CurrencyCode == _secondCurrency)
            {
                res = -res;
            }

            return res;
        }
    }
}