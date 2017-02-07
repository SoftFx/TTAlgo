using SoftFx.FxCalendar.Database;
using SoftFx.FxCalendar.Entities;
using SoftFx.FxCalendar.Models;

namespace SoftFx.FxCalendar.Storage
{
    public class FxStreetStorage : BaseStorage<FxStreetNewsModel, FxStreetNewsEntity>
    {
        public FxStreetStorage(string location, string currencyCode)
            : base(location, currencyCode)
        {
        }

        public override FxStreetNewsModel CreateEmptyModel()
        {
            return new FxStreetNewsModel(CurrencyCode, "", "");
        }

        public override DbContextBase<FxStreetNewsEntity> CreateDbContext()
        {
            return new FxStreetContext(Location, "FxStreet", CurrencyCode);
        }
    }
}