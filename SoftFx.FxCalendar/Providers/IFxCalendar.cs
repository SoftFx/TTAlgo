using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftFx.FxCalendar.Providers
{
    public interface IFxCalendare<T>
    {
        Filter Filter { get; }
        IEnumerable<T> FxNews { get; }
        void Download();
        Task DownloadTaskAsync();
        void DownloadAsync();
        IFxCalendare<T> AsDownloaded(Action action);
        IFxCalendare<T> AsFailure(Action action);

    }
}
