using System;
using SoftFx.FxCalendar.Common;
using SoftFx.FxCalendar.Sources.FxStreet;

namespace SoftFx.FxCalendar.Entities
{
    public class FxStreetNews : IFxStreetNews
    {
        public DateTime DateUtc { get; set; }
        public string Category { get; set; }
        public string Event { get; set; }
        public string Link { get; internal set; }
        public ImpactLevel Impact { get; set; }
        public string Actual { get; set; }
        public string Consensus { get; set; }
        public string Previous { get; set; }
        public string CountryCode { get; internal set; }
        public string CurrencyCode { get; internal set; }
    }
}
