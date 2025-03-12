using OnlineExamSystem.BL.Repositories;
using Microsoft.EntityFrameworkCore.Storage; // For IDbContextTransaction
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OnlineExamSystem.BL.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T> Repository<T>() where T : class;
        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        void Commit();
        void Rollback();
        Task DisposeAsync(); // Changed from Task ValueTask to Task for simplicity
        DbContext GetContext(); // Add this method
    }
}