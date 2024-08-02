using System.Linq.Expressions;
using vnvt_back_end.Application.Interfaces;
using vnvt_back_end.Application.Models;
using vnvt_back_end.Infrastructure.Contexts;

namespace vnvt_back_end.Infrastructure.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<PagedResult<Product>> GetPagedProductsAsync(PagingParameters pagingParameters, Expression<Func<Product, bool>> filter = null)
        {
            return await base.GetPagedAsync(pagingParameters, filter, x => x.CategoryId);
        }
        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await base.GetAllAsync(p => p.Category);
        }
    }
}