using SoftFx.FxCalendare.Data;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SoftFx.FxCalendar.Providers
{
    public class FxStreetRss : BaseFxCalendare<FxNews>
    {
        private string _rss = @"http://xml.fxstreet.com/fundamental/economic-calendar/events.xml";

        public override void Download()
        {
            try
            {
                using (var _webClient = new WebClient())
                {
                    var newsString = _webClient.DownloadString(ResolveSource());
                    FxNews = Parse(newsString);
                }

                ActionAfterDownloading();
            }
            catch (Exception ex)
            {
                ActionAfterFailure();
            }
        }

        public async override Task DownloadTaskAsync()
        {
            await Task.Run(() => Download());
        }

        public async override void DownloadAsync()
        {
            await Task.Run(() => Download());
        }

        private string ResolveSource()
        {
            return _rss;
        }

        private FxNews[] Parse(string data)
        {
            XDocument feedXML = XDocument.Parse(data);

            var feeds = feedXML.Descendants("item").Select(feed =>
            {
                var linkElement = feed.Element("link");
                var descriptionElement = feed.Element("description");
                var categoryElement = feed.Element("category");
                var date = DateTime.MinValue;
                var evnt = "";
                var actual = "";
                var consensus = "";
                var previous = "";

                if (descriptionElement != null)
                {
                    var regex = new Regex(Regex.Escape("<td>") + "(.*?)" + Regex.Escape("</td>"));
                    var mathes = regex.Matches(descriptionElement.Value);
                        /*
                        Date(GMT)       Event                         Cons.  Actual    Previous
                        May 18 21:00    Producer Price Index Growth          0.000 %   -0.100 %
                        */
                    date = DateTime.ParseExact(DateTime.UtcNow.Year + " " + mathes[5].Groups[1].Value, "yyyy MMM dd HH:mm", CultureInfo.InvariantCulture);
                    evnt = mathes[6].Groups[1].Value;
                    consensus = mathes[7].Groups[1].Value;
                    actual = mathes[8].Groups[1].Value;
                    previous = mathes[9].Groups[1].Value;
                }

                return new FxNews
                {
                    Link = linkElement != null ? linkElement.Value : "",
                    DateUtc = date,
                    Event = evnt,
                    Consensus = consensus,
                    Previous = previous,
                    Category = categoryElement != null ? categoryElement.Value : ""
                };
            }).ToArray();

            return feeds;
        }
    }
}

