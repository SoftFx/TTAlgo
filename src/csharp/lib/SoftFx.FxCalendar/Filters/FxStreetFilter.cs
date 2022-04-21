using System;
using SoftFx.FxCalendar.Calendar.FxStreet;

namespace SoftFx.FxCalendar.Filters
{
    public class FxStreetFilter : IFxStreetFilter
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ImpactLevel Impact { get; set; }
        public string[] CurrencyCodes { get; set; }

        public bool IsValid()
        {
            return (EndDate - StartDate).TotalDays >= 0 && CurrencyCodes.Length > 0;
        }
    }
}