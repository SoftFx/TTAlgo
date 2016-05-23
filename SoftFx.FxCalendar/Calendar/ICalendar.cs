using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SoftFx.FxCalendar.Filters;
using SoftFx.FxCalendar.Models;

namespace SoftFx.FxCalendar.Calendar
{
    public interface ICalendar<TModel, TFilter> where TModel : INews where TFilter : IFilter
    {
        TFilter Filter { get; }
        IEnumerable<TModel> News { get; }
        void Download();
        Task DownloadTaskAsync();
        void DownloadAsync();
        ICalendar<TModel, TFilter> AsDownloaded(Action action);
        ICalendar<TModel, TFilter> AsFailure(Action action);

    }
}
