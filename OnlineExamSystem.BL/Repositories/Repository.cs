// BL/Repositories/Repository.cs
using Microsoft.EntityFrameworkCore;
using OnlineExamSystem.Domains;
using System.Linq.Expressions;

namespace OnlineExamSystem.BL.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T> AddAsync(T entity) { await _dbSet.AddAsync(entity); return entity; }
        public async Task AddAsyncRange(IEnumerable<T> entities) { await _dbSet.AddRangeAsync(entities); await Task.CompletedTask; }
        public async Task UpdateAsync(T entity) { _dbSet.Update(entity); await Task.CompletedTask; }
        public async Task DeleteAsync(T entity) { _dbSet.Remove(entity); await Task.CompletedTask; }
        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public async Task<IQueryable<T>> GetAllIncludingAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return await Task.FromResult(query);
        }

        public async Task<List<T>> GetListAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();

        // New method to support nested includes
        public async Task<IQueryable<T>> GetAllWithNestedIncludesAsync(Func<IQueryable<T>, IQueryable<T>> includeFunc)
        {
            IQueryable<T> query = _dbSet;
            query = includeFunc(query);
            return await Task.FromResult(query);
        }
    }
}