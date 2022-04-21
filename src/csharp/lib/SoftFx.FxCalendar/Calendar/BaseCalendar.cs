using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SoftFx.FxCalendar.Filters;
using SoftFx.FxCalendar.Models;

namespace SoftFx.FxCalendar.Calendar
{
    public abstract class BaseCalendar<TModel, TFilter> : ICalendar<TModel, TFilter>
        where TModel : INews where TFilter : IFilter
    {
        private Action _afterDownloading;
        private Action _failDownloading;

        public TFilter Filter { get; protected set; }
        public IEnumerable<TModel> News { get; protected set; }

        protected BaseCalendar(TFilter filter)
        {
            Filter = filter;
            News = Enumerable.Empty<TModel>();
        }

        public ICalendar<TModel, TFilter> AsDownloaded(Action action)
        {
            _afterDownloading = action;
            return this;
        }

        public ICalendar<TModel, TFilter> AsFailure(Action action)
        {
            _failDownloading = action;
            return this;
        }

        public abstract void Download();
        public abstract void DownloadAsync();
        public abstract Task DownloadTaskAsync();

        protected void ActionAfterDownloading()
        {
            _afterDownloading?.Invoke();
        }

        protected void ActionAfterFailure()
        {
            _failDownloading?.Invoke();
        }
    }
}