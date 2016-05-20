using System.Data.Entity;
using System.Data.SQLite;
using SoftFx.FxCalendar.Models;
using SQLite.CodeFirst;

namespace SoftFx.FxCalendar.Database
{
    public abstract class DbContextBase<T> : DbContext where T : class, INews
    {
        public string Location { get; private set; }
        public string CalendarName { get; private set; }
        public string CurrencyCode { get; private set; }

        public DbSet<T> News { get; set; }

        protected DbContextBase(string location, string calendarName, string currencyCode)
            : base(SqLiteConnector.GetSqLiteConnection(location, calendarName, currencyCode), true)
        {
            Location = location;
            CalendarName = calendarName;
            CurrencyCode = currencyCode;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var model = modelBuilder.Build(Database.Connection);
            ISqlGenerator sqlGenerator = new SqliteSqlGenerator();
            string sql = sqlGenerator.Generate(model.StoreModel);
            Database.Connection.Open();
            var command = Database.Connection.CreateCommand();
            command.CommandText = sql;
            try
            {
                command.ExecuteNonQuery();
            }
            catch (SQLiteException e)
            {
                if (!(e.Message.Contains("table") && e.Message.Contains("already exists")))
                {
                    throw;
                }
            }
            Database.Connection.Close();
        }
    }
}