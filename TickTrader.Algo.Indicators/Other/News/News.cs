using System.Collections.Generic;
using SoftFx.FxCalendar.Calendar.FxStreet;
using SoftFx.FxCalendar.Filters;
using SoftFx.FxCalendar.Providers;
using SoftFx.FxCalendar.Storage;
using TickTrader.Algo.Api;
using SoftFx.FxCalendar.Models;
using System.Text;
using System.Linq;
using System;

namespace TickTrader.Algo.Indicators.Other.News
{
    [Indicator(Category = "Other", DisplayName = "News", Version = "1.0")]
    public class News : NewsMarkersBase
    {
        public new BarSeries Bars { get; set; }

        [Parameter(DefaultValue = "USD", DisplayName = "Currency")]
        public string Currency { get; set; }

        [Output(DisplayName = "Markers", Target = OutputTargets.Window1)]
        public DataSeries<Marker> Markers { get; set; }

        [Output(DisplayName = "Currency Impact", Target = OutputTargets.Window1, DefaultColor = Colors.Green, PlotType = PlotType.Histogram)]
        public DataSeries CurrencyImpact { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public News() { }

        public News(BarSeries bars)
        {
            Bars = bars;
        }

        public override void InitializeNewsProviders()
        {
            var currenciesList = new List<string>();

            if (Currency.Trim().Length == 3)
            {
                NewsProviders.Add(new FxStreetProvider(
                    new FxStreetCalendar(new FxStreetFilter()),
                    new FxStreetStorage("NewsCache", Currency),
                    ImpactLevel.None));
            }
            else
            {
                throw new ArgumentException("Invalid currency format");
            }
        }

        protected override void Draw()
        {
            if (News.Any())
            {
                MarkerText.Clear();

                CurrencyImpact[0] = (double)News.Max(x => (int)x.Impact);

                var sortedNews = News.OrderByDescending(news => news.Impact).ToList();
                var maxImpact = sortedNews.First().Impact;

                foreach (var n in sortedNews)
                {
                    if (!string.IsNullOrWhiteSpace(n.CurrencyCode))
                        MarkerText.Append(n.CurrencyCode.PadRight(4));

                    MarkerText.Append("[").Append(n.Impact).Append("] ")
                         .Append(n.Event).Append(" ")
                         .Append(n.Actual).AppendLine();
                }

                Markers[0].Y = maxImpact > 0? (double)maxImpact/2 : 1;
                Markers[0].Icon = MarkerIcons.Diamond;
                Markers[0].DisplayText = MarkerText.ToString();
                Markers[0].Color = ConvertToColor(maxImpact);
            }
        }
    }
}