using System.Data.Entity;
using System.Data.SQLite;
using SoftFx.FxCalendar.Models;

namespace SoftFx.FxCalendar.Database
{
    public class SqLiteInitializer<TEntity> : IDatabaseInitializer<DbContextBase<TEntity>>
        where TEntity : class, INews
    {
        public string CreateSql { get; protected set; }

        public SqLiteInitializer(string createSql)
        {
            CreateSql = createSql;
        }

        public void InitializeDatabase(DbContextBase<TEntity> context)
        {
            context.Database.Connection.Open();
            var command = context.Database.Connection.CreateCommand();
            command.CommandText = CreateSql;
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
            context.Database.Connection.Close();
        }
    }
}