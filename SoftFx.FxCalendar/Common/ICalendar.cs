using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SoftFx.FxCalendar.Utility;

namespace SoftFx.FxCalendar.Common
{
    public interface ICalendar<T> where T : INews
    {
        Filter Filter { get; }
        IEnumerable<T> FxNews { get; }
        void Download();
        Task DownloadTaskAsync();
        void DownloadAsync();
        ICalendar<T> AsDownloaded(Action action);
        ICalendar<T> AsFailure(Action action);

    }
}
