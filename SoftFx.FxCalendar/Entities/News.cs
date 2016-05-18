using System;
using SoftFx.FxCalendar.Common;

namespace SoftFx.FxCalendar.Entities
{
    public class News : INews
    {
        public DateTime DateUtc { get; set; }
        public ImpactLevel Impact { get; set; }
        public string CurrencyCode { get; internal set; }
    }
}