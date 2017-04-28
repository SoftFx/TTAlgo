using SoftFx.FxCalendar.Calendar.FxStreet;
using SoftFx.FxCalendar.Filters;
using SoftFx.FxCalendar.Models;
using SoftFx.FxCalendar.Providers;
using SoftFx.FxCalendar.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.Other.News
{
    [Indicator(Category = "Other", DisplayName = "News Marker", Version = "1.0")]
    public class NewsMarkersOverlay : NewsMarkersBase
    {
        [Parameter(DefaultValue = "", DisplayName = "Additional Currencies")]
        public string CurrencyCodes { get; set; }

        [Output(DisplayName = "Markers", Target = OutputTargets.Overlay)]
        public DataSeries<Marker> Markers { get; set; }

        public override void InitializeNewsProviders()
        {
            var currenciesList = new List<string>();

            currenciesList.Add(Symbol.BaseCurrency);
            currenciesList.Add(Symbol.CounterCurrency);

            currenciesList.AddRange(
                CurrencyCodes.Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s)));

            NewsProviders = currenciesList.Distinct().Select(c => new FxStreetProvider(
                new FxStreetCalendar(new FxStreetFilter()),
                new FxStreetStorage("NewsCache", c),
                ImpactLevel.None)).ToList();
        }

        protected override void Draw()
        {
            if (News.Any())
            {
                MarkerText.Clear();

                var sortedNews = News.OrderByDescending(news => (int)news.Impact).ToList();
                var maxImpact = sortedNews.First().Impact;

                foreach (var n in sortedNews)
                {
                    if (!string.IsNullOrWhiteSpace(n.CurrencyCode))
                        MarkerText.Append(n.CurrencyCode.PadRight(4));

                    MarkerText.Append("[").Append(n.Impact).Append("] ")
                         .Append(n.Event).Append(" ")
                         .Append(n.Actual).AppendLine();
                }

                Markers[0].Y = Bars.Median[0];
                Markers[0].Icon = MarkerIcons.Diamond;
                Markers[0].DisplayText = MarkerText.ToString();
                Markers[0].Color = ConvertToColor(maxImpact);
            }
        }
    }
}
