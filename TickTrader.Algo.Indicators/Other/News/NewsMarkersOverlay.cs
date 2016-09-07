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
    [Indicator(Category = "Other", DisplayName = "Other/News Marker", IsOverlay = true)]
    public class NewsMarkersOverlay : Indicator
    {
        private List<FxStreetProvider> providers;
        private StringBuilder markerTextBuilder = new StringBuilder();
        private List<FxStreetNewsModel> markerNewsList = new List<FxStreetNewsModel>();

        [Parameter(DefaultValue = "", DisplayName = "Additional Currencies")]
        public string AdCurrencies { get; set; }

        [Output(DisplayName = "Markers", DefaultColor = Colors.LightGreen)]
        public DataSeries<Marker> Markers { get; set; }

        protected override void Init()
        {
            var currenciesList = new List<string>();

            currenciesList.Add(Symbol.BaseCurrencyCode);
            currenciesList.Add(Symbol.CounterCurrencyCode);

            currenciesList.AddRange(
                AdCurrencies.Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s)));

            providers = currenciesList.Select(CreateProvider).ToList();
        }

        private FxStreetProvider CreateProvider(string currency)
        {
            var storage = new FxStreetStorage("NewsCache", currency);
            var calendar = new FxStreetCalendar(new FxStreetFilter());
            return new FxStreetProvider(calendar, storage, ImpactLevel.None);
        }

        protected override void Calculate()
        {
            markerNewsList.Clear();

            foreach (var provider in providers)
                markerNewsList.AddRange(GetCurrentNews(provider));

            DrawMarkers();
        }

        private void DrawMarkers()
        {
            if (markerNewsList.Count > 0)
            {
                markerTextBuilder.Clear();

                foreach (var n in markerNewsList)
                    markerTextBuilder
                        .Append(n.CurrencyCode).Append("  [")
                        .Append(n.Impact).Append("]  ")
                        .Append(n.Event).Append(" ")
                        .Append(n.Actual).AppendLine();

                Markers[0].Y = Bars.Median[0];
                Markers[0].Icon = MarkerIcons.Diamond;
                Markers[0].DisplayText = markerTextBuilder.ToString();

                var maxImpact = markerNewsList.Max(n => n.Impact);

                if (maxImpact == ImpactLevel.High)
                    Markers[0].Color = Colors.OrangeRed;
                else if (maxImpact == ImpactLevel.Medium)
                    Markers[0].Color = Colors.Green;
                else if (maxImpact == ImpactLevel.Low)
                    Markers[0].Color = Colors.Gray;
                else if (maxImpact == ImpactLevel.Medium)
                    Markers[0].Color = Colors.Gray;
            }
        }

        private IEnumerable<FxStreetNewsModel> GetCurrentNews(FxStreetProvider provider)
        {
            var from = Bars.Count > 1 ? Bars[1].CloseTime : Bars[0].OpenTime;
            var to = Bars[0].CloseTime;

            return provider.GetNews(from, to);
        }
    }
}
