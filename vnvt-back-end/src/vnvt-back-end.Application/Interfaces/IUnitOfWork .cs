using vnvt_back_end.Infrastructure;

namespace vnvt_back_end.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Product> ProductRepository { get; }
        IRepository<Payment> PaymentRepository { get; }
        IRepository<Order> OrderRepository { get; }
        IRepository<Review> ReviewRepository { get; }
        IRepository<Address> AddressRepository { get; }
        IRepository<Category> CategoryRepository { get; }
        IRepository<User> UserRepository { get; }
        IRepository<OrderItem> OrderItemRepository { get; }
        IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
        Task<int> CommitAsync();
    }
}

