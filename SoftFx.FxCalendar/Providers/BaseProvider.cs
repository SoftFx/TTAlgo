using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SoftFx.FxCalendar.Calendar;
using SoftFx.FxCalendar.Database;
using SoftFx.FxCalendar.Filters;
using SoftFx.FxCalendar.Models;
using SoftFx.FxCalendar.Storage;

namespace SoftFx.FxCalendar.Providers
{
    public abstract class BaseProvider<TModel, TEntity, TFilter> : IProvider<TModel, TEntity, TFilter>, IDisposable
        where TModel : INews, IModel<TEntity> where TEntity : class, INews where TFilter : IFilter
    {
        private static Dictionary<string, int> _storageAccessCnt = new Dictionary<string, int>();
        private string _storagePath;
        private bool _disposed;

        public const int DaysStep = 7;

        public ICalendar<TModel, TFilter> Calendar { get; protected set; }
        public IStorage<TModel, TEntity> Storage { get; protected set; }

        public BaseProvider(ICalendar<TModel, TFilter> calendar, IStorage<TModel, TEntity> storage)
        {
            Calendar = calendar;
            Storage = storage;

            _disposed = false;
            _storagePath = DbHelper.GetDbFilePath(Storage.DbContext.Location, Storage.DbContext.CalendarName, Storage.DbContext.CurrencyCode);
            if (_storageAccessCnt.ContainsKey(_storagePath))
                _storageAccessCnt[_storagePath] += 1;
            else _storageAccessCnt[_storagePath] = 1;
        }

        ~BaseProvider()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _storageAccessCnt[_storagePath] -= 1;

                _disposed = true;
            }
        }

        public IEnumerable<TModel> GetNews(DateTime start, DateTime end)
        {
            if (start > end)
            {
                return Enumerable.Empty<TModel>();
            }

            if (_storageAccessCnt[_storagePath] > 1)
            {
                Storage.ReloadNews();
            }

            if (end - Storage.LatestDate > TimeSpan.FromSeconds(1))
            {
                DownloadNews(Storage.LatestDate, end);
            }
            if (Storage.EarliestDate - start > TimeSpan.FromSeconds(1))
            {
                DownloadNews(start, Storage.EarliestDate);
            }

            return Storage.News.Where(m => m.DateUtc >= start && m.DateUtc < end);
        }

        protected void DownloadNews(DateTime start, DateTime end)
        {
            if (start > end)
            {
                return;
            }

            start = start.Date;
            if (DateTime.Today != end.Date)
            {
                end = end.Date - TimeSpan.FromSeconds(1);
            }

            for (var span = end - start; span.Days > DaysStep; span -= TimeSpan.FromDays(DaysStep))
            {
                _downloadNews(start, start + TimeSpan.FromDays(DaysStep) - TimeSpan.FromSeconds(1));
                start += TimeSpan.FromDays(DaysStep);
            }

            _downloadNews(start, end);
        }

        private void _downloadNews(DateTime start, DateTime end)
        {
            if (start > end)
            {
                return;
            }
            Debug.WriteLine("Downloading news from {0} to {1}", start, end);

            SetupFilter(start, end);

            Calendar.Download();

            if (Calendar.News != null)
            {
                Debug.WriteLine("Downloaded {0} news", Calendar.News.Count());
                Debug.WriteLine("Out of range {0} news", Calendar.News.Count(n => n.DateUtc < start || n.DateUtc > end));
                foreach (var newsModel in Calendar.News.Where(n => n.DateUtc < start || n.DateUtc > end))
                {
                    Debug.WriteLine($"{newsModel.DateUtc}");
                }
                Storage.AddNews(Calendar.News);
            }
        }

        protected abstract void SetupFilter(DateTime start, DateTime end);
    }
}