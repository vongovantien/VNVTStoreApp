using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vnvt_back_end.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
        Task<int> CommitAsync();
    }
}

