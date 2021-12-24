using System;

namespace SoftFx.FxCalendar.Filters
{
    public class NewsFilter : IFilter
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string[] CurrencyCodes { get; set; }

        public bool IsValid()
        {
            return (EndDate - StartDate).TotalDays >= 0 && CurrencyCodes.Length > 0;
        }
    }
}