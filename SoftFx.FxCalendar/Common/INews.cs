using System;

namespace SoftFx.FxCalendar.Common
{
    public interface INews
    {
        DateTime DateUtc { get; set; }
        ImpactLevel Impact { get; set; }
        string CurrencyCode { get; }
    }
}