using System;
using System.Collections.Generic;
using System.Linq;
using SoftFx.FxCalendar.Providers;
using SoftFx.FxCalendare.Data;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Utility;
using System.Text;

namespace TickTrader.Algo.Indicators.Other.News
{
    [Indicator(Category = "Other", DisplayName = "Other/News")]
    public class News : Indicator
    {
        private IEnumerable<FxNews> _news;
        private IShift _shifter;
        private bool isInitialized;

        [Parameter(DefaultValue = ImpactLevel.None, DisplayName = "Minimum Volatility")]
        public ImpactLevel MinVolatility { get; set; }

        [Parameter(DefaultValue = ImpactLevel.High, DisplayName = "Maximum Volatility")]
        public ImpactLevel MaxVolatility { get; set; }

        [Parameter(DefaultValue = 0, DisplayName = "Shift")]
        public int Shift { get; set; }

        [Parameter(DefaultValue = "EUR, USD")]
        public string Currencies { get; set; }

        [Input]
        public BarSeries Bars { get; set; }

        [Output(DisplayName = "Volatility Level", DefaultColor = Colors.SteelBlue)]
        public DataSeries VolatilityLevel { get; set; }

        [Output(DisplayName = "News Markers", DefaultColor = Colors.SteelBlue)]
        public DataSeries<Marker> Markers { get; set; }

        public int LastPositionChanged { get { return _shifter.Position; } }

        public News() { }

        public News(BarSeries bars, ImpactLevel minVolatility, ImpactLevel maxVolatility, int shift)
        {
            Bars = bars;
            MinVolatility = minVolatility;
            MaxVolatility = maxVolatility;
            Shift = shift;
        }

        protected override void Init()
        {
            _shifter = new SimpleShifter(Shift);
            _shifter.Init();
        }

        protected override void Calculate()
        {
            if (!isInitialized)
            {
                var currencies = Currencies.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s));
                LoadNews(currencies.ToArray());
                isInitialized = true;
            }

            var pos = 0;
            var level = 0;
            foreach (var n in _news.Where(n => n.DateUtc >= Bars[pos].OpenTime && n.DateUtc < Bars[pos].CloseTime))
            {
                if (n.Impact >= MinVolatility && n.Impact <= MaxVolatility)
                {
                    level += (int) n.Impact;
                }
            }
            if (IsUpdate)
            {
                _shifter.UpdateLast(level);
            }
            else
            {
                _shifter.Add(level);
            }
            VolatilityLevel[_shifter.Position] = _shifter.Result;

            var thisBarNews = SelectCurrent().ToList();

            if (thisBarNews.Count > 0)
            {
                var builder = new StringBuilder();

                foreach (var n in thisBarNews)
                    builder.Append(n.Impact).Append("  ").Append(n.Event).Append(" ").Append(n.Actual).AppendLine();

                var maxImpact = thisBarNews.Max(n => n.Impact);

                Markers[0].Y = 0.5;
                Markers[0].Icon = MarkerIcons.Diamond;
                Markers[0].DisplayText = builder.ToString();

                if (maxImpact == ImpactLevel.High)
                    Markers[0].Color = Colors.OrangeRed;
            }
        }

        private IEnumerable<FxNews> SelectCurrent()
        {
            return _news.Where(n => n.DateUtc >= Bars[1].CloseTime && n.DateUtc < Bars[0].CloseTime
                && n.Impact >= MinVolatility && n.Impact <= MaxVolatility);
        }

        private void LoadNews(string[] currencies)
        {
            var streetNewsProvider = new FxStreetCalendar();
            streetNewsProvider.Filter.StartDate = Bars[0].OpenTime.Date;
            streetNewsProvider.Filter.EndDate = DateTime.Today;
            streetNewsProvider.Filter.IsoCurrencyCodes = currencies;

            streetNewsProvider.DownloadTaskAsync().Wait();

            _news = streetNewsProvider.FxNews ?? Enumerable.Empty<FxNews>();
        }
    }
}
