using System;
using System.Collections.Generic;
using SoftFx.FxCalendar.Calendar;
using SoftFx.FxCalendar.Filters;
using SoftFx.FxCalendar.Models;
using SoftFx.FxCalendar.Storage;

namespace SoftFx.FxCalendar.Providers
{
    public interface IProvider<TModel, TEntity, TFilter>
        where TModel : INews, IModel<TEntity> where TEntity : class, INews where TFilter : IFilter
    {
        ICalendar<TModel, TFilter> Calendar { get; }
        IStorage<TModel, TEntity> Storage { get; }

        IEnumerable<TModel> GetNews(DateTime start, DateTime end);
    }
}