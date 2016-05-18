using System;
using System.Collections.Generic;
using System.Linq;
using SoftFx.FxCalendar.Common;
using SoftFx.FxCalendar.Entities;
using SoftFx.FxCalendar.Sources.FxStreet;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Other.News
{
    [Indicator(Category = "Other", DisplayName = "Other/News")]
    public class News : Indicator
    {
        private IEnumerable<FxStreetNews> _news;
        private IShift _firstShifter, _secondShifter;
        private string _firstCurrency, _secondCurrency;

        [Parameter(DefaultValue = ImpactLevel.None, DisplayName = "Minimum Volatility")]
        public ImpactLevel MinVolatility { get; set; }

        [Parameter(DefaultValue = ImpactLevel.High, DisplayName = "Maximum Volatility")]
        public ImpactLevel MaxVolatility { get; set; }

        [Parameter(DefaultValue = 0, DisplayName = "Shift")]
        public int Shift { get; set; }

        [Parameter(DefaultValue = "EURUSD", DisplayName = "Symbol Code")]
        public string SymbolCode { get; set; }

        [Input]
        public BarSeries Bars { get; set; }

        [Output(DisplayName = "First Currency Impact", DefaultColor = Colors.Green)]
        public DataSeries FirstCurrencyImpact { get; set; }

        [Output(DisplayName = "Second Currency Impact", DefaultColor = Colors.Red)]
        public DataSeries SecondCurrencyImpact { get; set; }

        public int LastPositionChanged { get { return _firstShifter.Position; } }

        public News() { }

        public News(BarSeries bars, ImpactLevel minVolatility, ImpactLevel maxVolatility, int shift)
        {
            Bars = bars;
            MinVolatility = minVolatility;
            MaxVolatility = maxVolatility;
            Shift = shift;

            InitializeIndicator();
        }

        private void InitializeIndicator()
        {
            _firstShifter = new SimpleShifter(Shift);
            _firstShifter.Init();
            _secondShifter = new SimpleShifter(Shift);
            _secondShifter.Init();
            _firstCurrency = SymbolCode.Substring(0, 3);
            _secondCurrency = SymbolCode.Substring(3, 3);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            if (Bars.Count == 1)
            {
                LoadNews();
            }
            var pos = 0;
            var firstLevel = 0;
            foreach (var n in _news.Where(n => n.DateUtc >= Bars[pos].OpenTime && n.DateUtc < Bars[pos].CloseTime && n.CurrencyCode == _firstCurrency))
            {
                if (n.Impact >= MinVolatility && n.Impact <= MaxVolatility)
                {
                    firstLevel += (int) n.Impact;
                }
            }

            var secondLevel = 0;
            foreach (var n in _news.Where(n => n.DateUtc >= Bars[pos].OpenTime && n.DateUtc < Bars[pos].CloseTime && n.CurrencyCode == _secondCurrency))
            {
                if (n.Impact >= MinVolatility && n.Impact <= MaxVolatility)
                {
                    secondLevel += (int)n.Impact;
                }
            }

            if (IsUpdate)
            {
                _firstShifter.UpdateLast(firstLevel);
                _secondShifter.UpdateLast(secondLevel);
            }
            else
            {
                _firstShifter.Add(firstLevel);
                _secondShifter.Add(secondLevel);
            }
            FirstCurrencyImpact[_firstShifter.Position] = _firstShifter.Result;
            SecondCurrencyImpact[_secondShifter.Position] = -_secondShifter.Result;

        }

        private void LoadNews()
        {
            var streetNewsProvider = new FxStreetCalendar
            {
                Filter =
                {
                    StartDate = Bars[0].OpenTime.Date,
                    EndDate = DateTime.Today,
                    CurrencyCodes = new[] {_firstCurrency, _secondCurrency}
                }
            };

            var task = streetNewsProvider.DownloadTaskAsync();

            task.Wait();

            _news = streetNewsProvider.FxNews ?? Enumerable.Empty<FxStreetNews>();
        }
    }
}
