using System.Data.Entity;
using SoftFx.FxCalendar.Entities;

namespace SoftFx.FxCalendar.Database
{
    public class NewsContext : DbContextBase
    {
        public  NewsContext(string location, string sourceName, string currencyCode)
            : base(location, sourceName, currencyCode)
        {
        }

        public DbSet<News> News { get; set; }
    }
}