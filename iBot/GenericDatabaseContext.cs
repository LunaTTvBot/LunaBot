using System;
using System.Data.Entity;
using NLog;
using SQLite.CodeFirst;
using System.Data.SQLite;

namespace IBot
{
    internal class GenericDatabaseContext<T> : DbContext
        where T : class
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public GenericDatabaseContext()
            : base(new SQLiteConnection()
            {
                ConnectionString = new SQLiteConnectionStringBuilder()
                {
                    DataSource = string.Format("{0}/iBot/gstore.{1}.db3",
                                               Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                               typeof(T).GUID.ToString("N")),
                    ForeignKeys = true,
                    Password = "secret",
                }.ConnectionString
            }, true) {}

        public DbSet<T> Table { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            try
            {
                var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<GenericDatabaseContext<T>>(modelBuilder);
                Database.SetInitializer(sqliteConnectionInitializer);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }
    }
}
