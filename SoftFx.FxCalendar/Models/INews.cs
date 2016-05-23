using System;

namespace SoftFx.FxCalendar.Models
{
    public interface INews
    {
        DateTime DateUtc { get; set; }
        string CurrencyCode { get; }
    }
}