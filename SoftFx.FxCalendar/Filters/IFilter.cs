using System;

namespace SoftFx.FxCalendar.Filters
{
    public interface IFilter
    {
        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
        string[] CurrencyCodes { get; set; }

        bool IsValid();
    }
}