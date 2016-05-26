using System;
using SoftFx.FxCalendar.Calendar;
using SoftFx.FxCalendar.Entities;
using SoftFx.FxCalendar.Filters;
using SoftFx.FxCalendar.Models;
using SoftFx.FxCalendar.Storage;

namespace SoftFx.FxCalendar.Providers
{
    public class NewsProvider : BaseProvider<NewsModel, NewsEntity, NewsFilter>
    {
        public NewsProvider(ICalendar<NewsModel, NewsFilter> calendar, IStorage<NewsModel, NewsEntity> storage)
            : base(calendar, storage)
        {
        }

        protected override void SetupFilter(DateTime start, DateTime end)
        {
            Calendar.Filter.CurrencyCodes = new[] { Storage.CurrencyCode };
            Calendar.Filter.StartDate = start;
            Calendar.Filter.EndDate = end;
        }
    }
}