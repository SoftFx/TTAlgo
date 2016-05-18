using SoftFx.FxCalendar.Common;
using SoftFx.FxCalendar.Database;

namespace SoftFx.FxCalendar.Storage
{
    public abstract class BaseStorage<T> : IStorage<T> where T: INews
    {
        public string Location { get; protected set; }
        public string CurrencyCode { get; protected set; }
        public ICalendar<T> Calendar { get; protected set; }
        public DbContextBase DbContext { get; protected set; }

        protected BaseStorage(string location, string currencyCode, ICalendar<T> calendar)
        {
            Location = location;
            CurrencyCode = currencyCode;
            Calendar = calendar;
            
            Initialize();
        }

        private void Initialize()
        {
            DbContext = CreateDbContext();
        }

        public abstract DbContextBase CreateDbContext();
    }
}
