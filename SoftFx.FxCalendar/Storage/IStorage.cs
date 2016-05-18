using SoftFx.FxCalendar.Common;
using SoftFx.FxCalendar.Database;

namespace SoftFx.FxCalendar.Storage
{
    public interface IStorage<T> where T : INews
    {
        string Location { get; }
        string CurrencyCode { get; }
        ICalendar<T> Calendar { get; }
        DbContextBase DbContext { get; }
    }
}