using vnvt_back_end.Application.Interfaces;
using vnvt_back_end.Infrastructure.Contexts;
using vnvt_back_end.Infrastructure.Repositories;

namespace vnvt_back_end.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IRepository<Product> _productRepository;
        private IRepository<Order> _orderRepository;
        private IRepository<Payment> _paymentRepository;
        private IRepository<Review> _reviewRepository;
        private IRepository<Address> _addressRepository;
        private IRepository<User> _userRepository;
        private IRepository<Category> _categoryRepository;
        private IRepository<OrderItem> _orderItemRepository;
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }


        public IRepository<Product> ProductRepository =>
            _productRepository ??= new Repository<Product>(_context);
        public IRepository<OrderItem> OrderItemRepository =>
            _orderItemRepository ??= new Repository<OrderItem>(_context);
        public IRepository<Category> CategoryRepository =>
            _categoryRepository ??= new Repository<Category>(_context);

        public IRepository<Order> OrderRepository =>
            _orderRepository ??= new Repository<Order>(_context);

        public IRepository<Payment> PaymentRepository =>
            _paymentRepository ??= new Repository<Payment>(_context);

        public IRepository<Review> ReviewRepository =>
            _reviewRepository ??= new Repository<Review>(_context);

        public IRepository<Address> AddressRepository =>
            _addressRepository ??= new Repository<Address>(_context);

        public IRepository<User> UserRepository =>
            _userRepository ??= new Repository<User>(_context);

        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            if (_repositories.ContainsKey(typeof(TEntity)))
            {
                return (IRepository<TEntity>)_repositories[typeof(TEntity)];
            }

            var repository = new Repository<TEntity>(_context);
            _repositories[typeof(TEntity)] = repository;
            return repository;
        }


        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
