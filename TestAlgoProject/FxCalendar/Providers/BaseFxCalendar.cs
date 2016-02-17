using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestAlgoProject.FxCalendar;

namespace SoftFx.FxCalendar.Providers
{
    public abstract class BaseFxCalendare<T> : IFxCalendare<T> where T : IFxNews
    {
        private Action _afterDownloading;
        private Action _failDownloading;

        public Filter Filter { get; private set; } = new Filter();
        public IEnumerable<T> FxNews { get; protected set; }

        public IFxCalendare<T> AsDownloaded(Action action)
        {
            _afterDownloading = action;
            return this;
        }

        public IFxCalendare<T> AsFailure(Action action)
        {
            _failDownloading = action;
            return this;
        }

        public abstract void Download();
        public abstract void DownloadAsync();
        public abstract Task DownloadTaskAsync();

        protected void ActionAfterDownloading()
        {
            if (_afterDownloading != null)
                _afterDownloading();
        }
        protected void ActionAfterFailure()
        {
            if (_failDownloading != null)
                _failDownloading();
        }
    }
}
