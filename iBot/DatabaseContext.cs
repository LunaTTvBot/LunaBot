using System.Data.Entity;
using SQLite.CodeFirst;

namespace IBot
{
    internal class DatabaseContext : DbContext
    {
        public DatabaseContext()
            : base("Store") {}

        public DbSet<Channel> History { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<DatabaseContext>(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);
        }
    }
}
