using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using SoftFx.FxCalendar.Filters;
using SoftFx.FxCalendar.Models;
using SoftFx.FxCalendar.Utility;

namespace SoftFx.FxCalendar.Calendar.FxStreet
{
    public class FxStreetCalendar : BaseCalendar<FxStreetNewsModel, FxStreetFilter>
    {
        private const int DateIndex = 0;
        private const int TimeIndex = 0;
        private const int CountryCodeIndex = 1;
        private const int CurrencyCodeIndex = 2;
        private const int EventIndex = 3;
        private const int VolatilityIndex = 4;
        private const int ActualIndex = 5;
        private const int ConsensusIndex = 6;
        private const int PreviousIndex = 7;

        private string _hostUrl = @"http://calendar.fxstreet.com/EventDateWidget/GetMini";

        public FxStreetCalendar(FxStreetFilter filter) : base(filter)
        {
        }

        public override void Download()
        {
            try
            {
                if (!Filter.IsValid())
                {
                    News = Enumerable.Empty<FxStreetNewsModel>();
                    return;
                }

                using (WebClient wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    var fxnews = new List<FxStreetNewsModel>();
                    foreach (var datePeriod in CalculateDatePeriods())
                    {
                        var requestParams = ResolveStringParams(datePeriod);
                        var rawResponse = wc.UploadString(_hostUrl, requestParams);
                        fxnews.AddRange(Parse(rawResponse, datePeriod.Item2.Year));
                    }
                    News = fxnews;
                }

                ActionAfterDownloading();
            }
            catch (Exception)
            {
                ActionAfterFailure();
            }
        }

        public override Task DownloadTaskAsync()
        {
            return Task.Run(() => Download());
        }

        public override async void DownloadAsync()
        {
            await Task.Run(() => Download());
        }

        private IEnumerable<FxStreetNewsModel> Parse(string data, int year)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(data);

            var table = doc.DocumentNode
                .Descendants("tr")
                .Select(n => n.Elements("td").Select(e => e.InnerText).ToArray());

            var news = new List<FxStreetNewsModel>();

            var dateNews = DateTime.MinValue;
            foreach (var tr in table)
            {
                if (tr.Length == 1)
                {
                    DateTime.TryParse(tr[DateIndex] + " " + year, out dateNews);
                }
                else if (tr.Length > 1)
                    news.Add(
                        new FxStreetNewsModel(tr[CurrencyCodeIndex].Trim(), "", tr[CountryCodeIndex].Trim())
                        {
                            DateUtc = ConcatDateAndTime(dateNews, tr[TimeIndex]),
                            Event = tr[EventIndex].Trim(),
                            Impact = (ImpactLevel) int.Parse(tr[VolatilityIndex].Trim()),
                            Actual = HttpUtility.HtmlDecode(tr[ActualIndex].Trim()),
                            Consensus = HttpUtility.HtmlDecode(tr[ConsensusIndex].Trim()),
                            Previous = HttpUtility.HtmlDecode(tr[PreviousIndex].Trim())
                        });
            }
            return news.ToArray();
        }

        private DateTime ConcatDateAndTime(DateTime dateNews, string time)
        {
            TimeSpan tsp;
            if (TimeSpan.TryParse(time, out tsp))
            {
                return dateNews.Add(tsp);
            }
            return dateNews;
        }

        private string ResolveStringParams(Tuple<DateTime, DateTime> datePeriod)
        {
            var sBuilder = new StringBuilder();
            sBuilder.Append("start=" + datePeriod.Item1.ToString("yyyyMMdd"));
            sBuilder.AppendFormat("&end=" + datePeriod.Item2.ToString("yyyyMMdd"));
            sBuilder.AppendFormat("&volatility={0}", (int) Filter.Impact);
            sBuilder.Append("&culture=en-US&");
            sBuilder.Append("&timezone=UTC");
            sBuilder.Append("&view=range");
            sBuilder.Append(
                "&columns=date%2Ctime%2Ccountry%2Cevent%2Cactual%2Cconsensus%2Cprevious%2Cvolatility%2Ccountrycurrency");
            var countriesFilter = string.Join(",",
                Filter.CurrencyCodes.SelectMany(c => RegionInfoUtility.CurrencyToCountries[c]).ToArray());
            sBuilder.Append("&countrycode=" + HttpUtility.UrlEncode(countriesFilter));

            return sBuilder.ToString();
        }

        private IEnumerable<Tuple<DateTime, DateTime>> CalculateDatePeriods()
        {
            var dateStart = Filter.StartDate;
            var dateEnd = Filter.EndDate;
            while (dateStart.Year - dateEnd.Year < 0)
            {
                var lastDayOfTheYear = new DateTime(dateStart.Year, 12, 31);
                var resultTuple = new Tuple<DateTime, DateTime>(dateStart, lastDayOfTheYear);
                dateStart = lastDayOfTheYear.AddDays(1);
                yield return resultTuple;
            }

            yield return new Tuple<DateTime, DateTime>(dateStart, dateEnd);
        }
    }
}