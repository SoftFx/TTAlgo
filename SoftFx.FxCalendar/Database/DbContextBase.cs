using System.Data.Entity;

namespace SoftFx.FxCalendar.Database
{
    public class DbContextBase : DbContext
    {
        public string Location { get; private set; }
        public string SourceName { get; private set; }
        public string CurrencyCode { get; private set; }

        protected DbContextBase(string location, string sourceName, string currencyCode)
            : base(DbConnector.GetConnectionString(location, sourceName, currencyCode))
        {
            Location = location;
            SourceName = sourceName;
            CurrencyCode = currencyCode;
        }
    }
}