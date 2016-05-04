using System;
using System.Collections.Generic;
using System.Linq;
using SoftFx.FxCalendar.Providers;
using SoftFx.FxCalendare.Data;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Other.News
{
    [Indicator(Category = "Other", DisplayName = "Other/News")]
    public class News : Indicator
    {
        private IEnumerable<FxNews> _news;
        private IShift _shifter;

        [Parameter(DefaultValue = ImpactLevel.None, DisplayName = "Minimum Volatility")]
        public ImpactLevel MinVolatility { get; set; }

        [Parameter(DefaultValue = ImpactLevel.High, DisplayName = "Maximum Volatility")]
        public ImpactLevel MaxVolatility { get; set; }

        [Parameter(DefaultValue = 0, DisplayName = "Shift")]
        public int Shift { get; set; }

        [Input]
        public BarSeries Bars { get; set; }

        [Output(DisplayName = "Volatility Level", DefaultColor = Colors.SteelBlue)]
        public DataSeries VolatilityLevel { get; set; }

        public int LastPositionChanged { get { return _shifter.Position; } }

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
            _shifter = new SimpleShifter(Shift);
            _shifter.Init();
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

        }

        private void LoadNews()
        {
            var streetNewsProvider = new FxStreetCalendar();
            streetNewsProvider.Filter.StartDate = Bars[0].OpenTime.Date;
            streetNewsProvider.Filter.EndDate = DateTime.Today;
            streetNewsProvider.Filter.IsoCurrencyCodes = new[]
            //{Bars.SymbolCode.Substring(0, 3), Bars.SymbolCode.Substring(3)};
            {"EUR", "USD"};

            var task = streetNewsProvider.DownloadTaskAsync();

            task.Wait();

            _news = streetNewsProvider.FxNews ?? Enumerable.Empty<FxNews>();
        }
    }
}
