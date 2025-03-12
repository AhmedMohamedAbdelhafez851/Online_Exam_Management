using OnlineExamSystem.Domains;
using OnlineExamSystem.BL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace OnlineExamSystem.BL.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly Dictionary<Type, object> _repositories = new();
        private bool _disposed;
        private IDbContextTransaction _transaction;

        public UnitOfWork(ApplicationDbContext context) => _context = context;

        public IRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);
            if (!_repositories.TryGetValue(type, out var repo))
            {
                repo = new Repository<T>(_context);
                _repositories[type] = repo;
            }
            return (IRepository<T>)repo;
        }

        public DbContext GetContext() => _context; // Add this method

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
            return _transaction;
        }

        public void Commit()
        {
            _transaction?.Commit();
        }

        public void Rollback()
        {
            _transaction?.Rollback();
            _transaction?.Dispose();
            _transaction = null!;
        }

        public async Task DisposeAsync()
        {
            if (!_disposed)
            {
                _transaction?.Dispose();
                await _context.DisposeAsync();
                _repositories.Clear();
                _disposed = true;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _transaction?.Dispose();
                _context.Dispose();
                _repositories.Clear();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}