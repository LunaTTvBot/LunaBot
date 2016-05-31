using System.Data.Entity;
using IBot.Models;
using SQLite.CodeFirst;

namespace IBot.Database
{
    internal class DatabaseContext : DbContext
    {
        private static DatabaseContext _instance;
        private static readonly object InstanceLock = new object();

        private DatabaseContext()
            : base("Store") {}

        public DbSet<DbChannel> HistoryChannels { get; set; }
        public DbSet<DbUser> HistoryUsers { get; set; }
        public DbSet<Extension> ObjectExtensions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<DatabaseContext>(modelBuilder);
            System.Data.Entity.Database.SetInitializer(sqliteConnectionInitializer);
            modelBuilder.Entity<Extension>().HasKey(t => new { t.ClassName, t.Id, t.PropertyName });
        }

        public static DatabaseContext Get()
        {
            lock (InstanceLock)
            {
                return _instance ?? (_instance = new DatabaseContext());
            }
        }
    }
}
