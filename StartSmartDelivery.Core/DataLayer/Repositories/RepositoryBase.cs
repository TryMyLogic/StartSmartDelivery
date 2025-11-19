using Microsoft.EntityFrameworkCore;
using StartSmartDelivery.Core.DataLayer.Data;
using StartSmartDelivery.Core.DataLayer.Repositories.Interfaces;

namespace StartSmartDelivery.Core.DataLayer.Repositories
{
    public class RepositoryBase<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public RepositoryBase(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(params object[] keyValues)
        {
            T? entity = await GetByIdAsync(keyValues);
            if (entity is not null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(params object[] keyValues)
        {
            return await _dbSet.FindAsync(keyValues);
        }

        /// <summary>
        /// Provides an IQueryable base for flexible query construction in the Service Layer.
        /// </summary>
        /// <remarks>
        /// Uses AsNoTracking() as a default optimization for read-only queries.
        /// For updates or modifications, use AsTracking() on the returned IQueryable to enable EF Core change tracking.
        /// </remarks>
        public IQueryable<T> GetQueryable()
        {
            return _dbSet.AsNoTracking();
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
