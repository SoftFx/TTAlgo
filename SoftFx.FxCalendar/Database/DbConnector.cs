using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;

namespace SoftFx.FxCalendar.Database
{
    public static class DbConnector
    {
        public const string AppName = "EntityFramework";
        public const string ProviderName = "System.Data.SQLite.EF6";

        public static string GetConnectionString(string location, string sourceName, string currencyCode)
        {
            return new EntityConnectionStringBuilder
            {
                Metadata = "",
                Provider = ProviderName,
                ProviderConnectionString = new SqlConnectionStringBuilder
                {
                    ApplicationName = AppName,
                    InitialCatalog = location,
                    DataSource = $"{sourceName}-{currencyCode}.db",
                    IntegratedSecurity = true
                }.ConnectionString
            }.ConnectionString;
        }
    }
}