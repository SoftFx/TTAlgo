using SoftFx.FxCalendar.Entities;

namespace SoftFx.FxCalendar.Database
{
    public class FxStreetContext : DbContextBase<FxStreetNewsEntity>
    {
        public FxStreetContext(string location, string calendarName, string currencyCode)
            : base(location, calendarName, currencyCode)
        {
        }
    }
}