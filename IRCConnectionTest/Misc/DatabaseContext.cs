using System.Data.Entity;
using SQLite.CodeFirst;

namespace IRCConnectionTest.Misc
{
    internal class DatabaseContext : DbContext
    {
        private static DatabaseContext _instance;

        public DbSet<User> Users { get; set; }
        public DbSet<Channel> Channels { get; set; }

        public DatabaseContext()
            : base("Store") {}

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<DatabaseContext>(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);
        }

        public static DatabaseContext Get() => _instance ?? (_instance = new DatabaseContext());

        public static int Save() => Get().SaveChanges();
    }
}
