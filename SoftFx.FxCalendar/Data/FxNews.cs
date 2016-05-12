using System;

namespace SoftFx.FxCalendare.Data
{
    public class FxNews : IFxNews
    {
        public DateTime DateUtc { get; set; }
        public string Category { get; set; }
        public string Event { get; set; }
        public string Link { get; internal set; }
        public ImpactLevel Impact { get; set; }
        public string Actual { get; set; }
        public string Consensus { get; set; }
        public string Previous { get; set; }
        public object IsoCountryCode { get; internal set; }
        public object IsoCurrencyCode { get; internal set; }
    }
}
