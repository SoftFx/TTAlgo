using System.Data.Entity;
using SoftFx.FxCalendar.Models;
using SQLite.CodeFirst;

namespace SoftFx.FxCalendar.Database
{
    public abstract class DbContextBase<TEntity> : DbContext where TEntity : class, INews
    {
        private static SqLiteInitializer<TEntity> _initializer;

        public string Location { get; protected set; }
        public string CalendarName { get; protected set; }
        public string CurrencyCode { get; protected set; }

        public DbSet<TEntity> News { get; set; }

        protected DbContextBase(string location, string calendarName, string currencyCode)
            : base(SqLiteConnector.GetSqLiteConnection(location, calendarName, currencyCode), true)
        {
            Location = location;
            CalendarName = calendarName;
            CurrencyCode = currencyCode;
            _initializer?.InitializeDatabase(this);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var model = modelBuilder.Build(Database.Connection);
            ISqlGenerator sqlGenerator = new SqliteSqlGenerator();
            var createSql = sqlGenerator.Generate(model.StoreModel);
            _initializer = new SqLiteInitializer<TEntity>(createSql);
            _initializer.InitializeDatabase(this);
        }
    }
}