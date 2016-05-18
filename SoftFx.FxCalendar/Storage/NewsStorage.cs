using SoftFx.FxCalendar.Common;
using SoftFx.FxCalendar.Database;

namespace SoftFx.FxCalendar.Storage
{
    public class NewsStorage : BaseStorage<INews>
    {
        public NewsStorage(string location, string currencyCode, ICalendar<INews> calendar)
            : base(location, currencyCode, calendar)
        {
        }

        public override DbContextBase CreateDbContext()
        {
            return new NewsContext(Location, "News", CurrencyCode);
        }
    }
}