using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using IBot.Database;
using System.Threading;
using System.Threading.Tasks;

namespace IBot.Facades.Core
{
    public class GenericDatabase<T> : IDisposable
        where T : class
    {
        private readonly GenericDatabaseContext<T> _context;

        public DbSet<T> Table
        {
            get { return _context.Table; }
            set { _context.Table = value; }
        }

        public System.Data.Entity.Database Database => _context.Database;

        public DbChangeTracker DbChangeTracker => _context.ChangeTracker;

        public DbContextConfiguration Configuration => _context.Configuration;

        public GenericDatabase()
        {
            _context = new GenericDatabaseContext<T>();
        }

        public void Entry(object entity) => _context.Entry(entity);

        public void Entry<TEntity>(TEntity entity)
            where TEntity : class
            => _context.Entry<TEntity>(entity);

        public IEnumerable<DbEntityValidationResult> GetValidationErrors() => _context.GetValidationErrors();

        public int SaveChanges() => _context.SaveChanges();

        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => _context.SaveChangesAsync(cancellationToken);

        public DbSet Set(Type entityType) => _context.Set(entityType);

        public DbSet Set<TEntity>()
            where TEntity : class
            => _context.Set<TEntity>();

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
