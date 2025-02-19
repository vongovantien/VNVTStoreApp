using FW.WAPI.Core.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Uow
{
    public interface IUnitOfWork<TDataContext, TEntity> : IDisposable
        where TDataContext : DbContext
        where TEntity : class
    {
        Task<int> SaveChangesAsync();

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        void SaveChanges();

        IRepository<TDataContext, TEntity> GetRepository();

        IRepository<TDataContext, TEntity> GetRepository(Type type);
    }
}