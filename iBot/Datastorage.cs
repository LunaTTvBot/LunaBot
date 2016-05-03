using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.CodeFirst;

namespace IBot
{
    internal class Datastorage : DbContext
    {
        public Datastorage()
            : base("Store") {}

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<Datastorage>(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);
        }
    }
}
