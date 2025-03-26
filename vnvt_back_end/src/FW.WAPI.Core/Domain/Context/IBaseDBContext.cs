using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Domain.Context
{
    public interface IBaseDBContext : IDisposable
    {
        DbSet<TEntity> Set<TEntity>() where TEntity : class;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));

        int SaveChanges();

        EntityEntry Entry(object entity);

        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

        IModel Model { get; }
        DatabaseFacade Database { get; }

        string GetUniqueCodePrefix(string tableName, ref int maxLength, string companyCode = null);
    }
}