using System;
using System.Collections.Generic;
using SoftFx.FxCalendar.Database;
using SoftFx.FxCalendar.Models;

namespace SoftFx.FxCalendar.Storage
{
    public interface IStorage<TModel, TEntity> where TModel : INews, IModel<TEntity> where TEntity : class, INews
    {
        string Location { get; }
        string CurrencyCode { get; }
        DbContextBase<TEntity> DbContext { get; }
        List<TModel> News { get; }
        DateTime EarliestDate { get; }
        DateTime LatestDate { get; }

        void AddNews(IEnumerable<TModel> news);
        void ReloadNewsIfNeeded();
        void UpdateDatesRange(DateTime start, DateTime end);
    }
}