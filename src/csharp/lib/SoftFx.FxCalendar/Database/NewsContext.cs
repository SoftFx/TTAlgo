using SoftFx.FxCalendar.Entities;

namespace SoftFx.FxCalendar.Database
{
    public class NewsContext : DbContextBase<NewsEntity>
    {
        public  NewsContext(string location, string calendarName, string currencyCode)
            : base(location, calendarName, currencyCode)
        {
        }
    }
}