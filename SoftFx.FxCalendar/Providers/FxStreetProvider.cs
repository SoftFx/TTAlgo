using System;
using SoftFx.FxCalendar.Calendar;
using SoftFx.FxCalendar.Calendar.FxStreet;
using SoftFx.FxCalendar.Entities;
using SoftFx.FxCalendar.Filters;
using SoftFx.FxCalendar.Models;
using SoftFx.FxCalendar.Storage;

namespace SoftFx.FxCalendar.Providers
{
    public class FxStreetProvider : BaseProvider<FxStreetNewsModel, FxStreetNewsEntity, FxStreetFilter>
    {
        public ImpactLevel MinimalImpact {get; set; }

        public FxStreetProvider(ICalendar<FxStreetNewsModel, FxStreetFilter> calendar,
            IStorage<FxStreetNewsModel, FxStreetNewsEntity> storage, ImpactLevel minimalImpact)
            : base(calendar, storage)
        {
            MinimalImpact = minimalImpact;
        }

        protected override void SetupFilter(DateTime start, DateTime end)
        {
            Calendar.Filter.CurrencyCodes = new[] { Storage.CurrencyCode };
            Calendar.Filter.StartDate = start;
            Calendar.Filter.EndDate = end;
            Calendar.Filter.Impact = MinimalImpact;
        }
    }
}