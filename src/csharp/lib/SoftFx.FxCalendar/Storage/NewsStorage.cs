using SoftFx.FxCalendar.Database;
using SoftFx.FxCalendar.Entities;
using SoftFx.FxCalendar.Models;

namespace SoftFx.FxCalendar.Storage
{
    public class NewsStorage : BaseStorage<NewsModel, NewsEntity>
    {
        public NewsStorage(string location, string currencyCode)
            : base(location, currencyCode)
        {
        }

        public override NewsModel CreateEmptyModel()
        {
            return new NewsModel(CurrencyCode);
        }

        public override DbContextBase<NewsEntity> CreateDbContext()
        {
            return new NewsContext(Location, "News", CurrencyCode);
        }
    }
}