using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SoftFx.FxCalendar.Database;
using SoftFx.FxCalendar.Models;

namespace SoftFx.FxCalendar.Storage
{
    public abstract class BaseStorage<TModel, TEntity> : IStorage<TModel, TEntity>
        where TModel : INews, IModel<TEntity> where TEntity : class, INews
    {
        private static Dictionary<string, int> _storageAccessCnt = new Dictionary<string, int>();
        private string _storagePath;
        private int _storageVersion;

        public string Location { get; protected set; }
        public string CurrencyCode { get; protected set; }
        public DbContextBase<TEntity> DbContext { get; protected set; }
        public List<TModel> News { get; protected set; }
        public DateTime EarliestDate { get; protected set; }
        public DateTime LatestDate { get; protected set; }

        protected BaseStorage(string location, string currencyCode)
        {
            Location = location;
            CurrencyCode = currencyCode;

            Initialize();
        }

        private void Initialize()
        {
            var dbDir = DbHelper.GetDbDirectory(Location);
            if (!Directory.Exists(dbDir))
            {
                Directory.CreateDirectory(dbDir);
            }

            DbContext = CreateDbContext();

            _storagePath = DbHelper.GetDbFilePath(DbContext.Location, DbContext.CalendarName, DbContext.CurrencyCode);
            if (!_storageAccessCnt.ContainsKey(_storagePath))
                _storageAccessCnt[_storagePath] = 1;

            LoadNewsFromDb();
        }

        protected void LoadNewsFromDb()
        {
            News = new List<TModel>();
            _storageVersion = _storageAccessCnt[_storagePath];

            if (!DbContext.News.Any())
            {
                EarliestDate = DateTime.Now.ToUniversalTime();
                LatestDate = DateTime.Now.ToUniversalTime();
            }
            else
            {
                EarliestDate = DbContext.News.First().DateUtc;
                LatestDate = EarliestDate;
            }

            foreach (var entity in DbContext.News)
            {
                var model = CreateEmptyModel();
                model.InitFromEntity(entity);
                News.Add(model);
                UpdateDatesRange(model);
            }
        }

        protected void UpdateDatesRange(TModel model)
        {
            EarliestDate = model.DateUtc.Date < EarliestDate ? model.DateUtc.Date : EarliestDate;
            LatestDate = model.DateUtc.Date >= LatestDate
                ? model.DateUtc.Date + TimeSpan.FromDays(1)
                : LatestDate;
        }

        public abstract TModel CreateEmptyModel();
        public abstract DbContextBase<TEntity> CreateDbContext();

        public void AddNews(IEnumerable<TModel> news)
        {
            ReloadNewsIfNeeded();

            foreach (var model in news)
            {
                News.Add(model);
                DbContext.News.Add(model.ConvertToEntity());
                UpdateDatesRange(model);
            }
            DbContext.SaveChanges();

            _storageAccessCnt[_storagePath]++;
            _storageVersion = _storageAccessCnt[_storagePath];
        }

        public void ReloadNewsIfNeeded()
        {
            if (_storageVersion != _storageAccessCnt[_storagePath])
            {
                LoadNewsFromDb();
            }
        }

        public void UpdateDatesRange(DateTime start, DateTime end)
        {
            EarliestDate = start.Date < EarliestDate ? start.Date : EarliestDate;
            LatestDate = end.Date >= LatestDate
                ? end.Date + TimeSpan.FromDays(1)
                : LatestDate;
        }
    }
}
