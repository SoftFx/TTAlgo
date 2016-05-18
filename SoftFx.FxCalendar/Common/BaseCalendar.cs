using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SoftFx.FxCalendar.Utility;

namespace SoftFx.FxCalendar.Common
{
    public abstract class BaseCalendar<T> : ICalendar<T> where T : INews
    {
        private Action _afterDownloading;
        private Action _failDownloading;

        public Filter Filter { get; private set; } = new Filter();
        public IEnumerable<T> FxNews { get; protected set; }

        public ICalendar<T> AsDownloaded(Action action)
        {
            _afterDownloading = action;
            return this;
        }

        public ICalendar<T> AsFailure(Action action)
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
