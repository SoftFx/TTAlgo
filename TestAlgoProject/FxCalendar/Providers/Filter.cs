using System;
using TestAlgoProject.FxCalendar;

namespace SoftFx.FxCalendar.Providers
{
    public class Filter
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ImpactLevel Impact { get; set; }
        public string[] IsoCurrencyCodes { get; set; }

        internal bool IsValid()
        {
            return (EndDate - StartDate).TotalDays >= 0 && IsoCurrencyCodes.Length > 0;
        }
    }
}