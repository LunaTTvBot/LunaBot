using System.Data.Entity;
using SQLite.CodeFirst;

namespace IBot
{
    internal class DatabaseContext : DbContext
    {
        private static DatabaseContext _instance;

        private DatabaseContext()
            : base("Store") {}

        public DbSet<DbChannel> HistoryChannels { get; set; }
        public DbSet<DbUser> HistoryUsers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<DatabaseContext>(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);
        }

        public static DatabaseContext Get() => _instance ?? (_instance = new DatabaseContext());
    }
}
