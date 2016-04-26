using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.CodeFirst;

namespace IRCConnectionTest.Misc
{
    class DatabaseContext : DbContext
    {
        private static DatabaseContext Instance;

        public DbSet<User> Users { get; set; }

        public DatabaseContext()
            : base("Store")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<DatabaseContext>(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);
        }

        public static DatabaseContext Get()
        {
            if (Instance == null)
                Instance = new DatabaseContext();
            return Instance;
        }
    }
}
