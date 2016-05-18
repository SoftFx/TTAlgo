using System;
using SoftFx.FxCalendar.Common;

namespace SoftFx.FxCalendar.Utility
{
    public class Filter
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ImpactLevel Impact { get; set; }
        public string[] CurrencyCodes { get; set; }

        internal bool IsValid()
        {
            return (EndDate - StartDate).TotalDays >= 0 && CurrencyCodes.Length > 0;
        }
    }
}