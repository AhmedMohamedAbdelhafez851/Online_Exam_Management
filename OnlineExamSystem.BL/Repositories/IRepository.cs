// BL/Repositories/IRepository.cs
using System.Linq.Expressions;

namespace OnlineExamSystem.BL.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T> AddAsync(T entity);
        Task AddAsyncRange(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<T?> GetByIdAsync(int id);
        Task<IQueryable<T>> GetAllIncludingAsync(params Expression<Func<T, object>>[] includes);
        Task<List<T>> GetListAsync(Expression<Func<T, bool>> predicate);
        Task<IQueryable<T>> GetAllWithNestedIncludesAsync(Func<IQueryable<T>, IQueryable<T>> includeFunc); // New method
    }
}