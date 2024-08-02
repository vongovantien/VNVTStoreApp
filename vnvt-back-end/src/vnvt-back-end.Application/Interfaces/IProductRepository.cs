using System.Linq.Expressions;
using vnvt_back_end.Application.Models;
using vnvt_back_end.Infrastructure;

namespace vnvt_back_end.Application.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<PagedResult<Product>> GetPagedProductsAsync(PagingParameters pagingParameters, Expression<Func<Product, bool>> filter = null);
        Task<IEnumerable<Product>> GetAllProductsAsync();
    }
}
