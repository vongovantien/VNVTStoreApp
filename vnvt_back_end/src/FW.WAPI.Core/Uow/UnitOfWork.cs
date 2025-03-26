using FW.WAPI.Core.ExceptionHandling;
using FW.WAPI.Core.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FW.WAPI.Core.Uow
{
    public class UnitOfWork<TDataContext, TEntity> : IUnitOfWork<TDataContext, TEntity>, IDisposable
        where TDataContext : DbContext
        where TEntity : class
    {
        protected readonly IServiceProvider _serviceProvider;
        public readonly TDataContext _dataContext;

        public UnitOfWork(TDataContext dataContext, IServiceProvider serviceProvider)
        {
            _dataContext = dataContext;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        ///
        /// </summary>
        public void SaveChanges()
        {
            _dataContext.SaveChanges();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public Task<int> SaveChangesAsync()
        {
            return _dataContext.SaveChangesAsync();
        }

        /// <summary>
        ///
        /// </summary>
        public void Commit()
        {
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return _dataContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public IRepository<TDataContext, TEntity> GetRepository()
        {
            var repositoryType = typeof(TEntity);
            var repository = (IRepository<TDataContext, TEntity>)_serviceProvider.GetService(repositoryType);
            if (repository == null)
            {
                throw new RepositoryNotFoundException(repositoryType.Name,
                     string.Format("Repository {0} not found in the IOC container. Check if it is registered during startup.", repositoryType.Name));
            }

            return repository;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IRepository<TDataContext, TEntity> GetRepository(Type type)
        {
            var repositoryType = type;
            var repository = (IRepository<TDataContext, TEntity>)_serviceProvider.GetService(repositoryType);
            if (repository == null)
            {
                throw new RepositoryNotFoundException(repositoryType.Name, string.Format("Repository {0} not found in the IOC container. Check if it is registered during startup.", repositoryType.Name));
            }

            return repository;
        }
    }
}