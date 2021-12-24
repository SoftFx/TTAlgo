using System.Data.SQLite;

namespace SoftFx.FxCalendar.Database
{
    public static class SqLiteConnector
    {
        public const string AppName = "EntityFramework";
        public const string ProviderName = "System.Data.SQLite";

        public static SQLiteConnection GetSqLiteConnection(string location, string calendarName, string currencyCode)
        {
            return
                new SQLiteConnection(
                    new SQLiteConnectionStringBuilder
                    {
                        DataSource = DbHelper.GetDbFilePath(location, calendarName, currencyCode),
                        ForeignKeys = true
                    }.ConnectionString);
        }
    }
}