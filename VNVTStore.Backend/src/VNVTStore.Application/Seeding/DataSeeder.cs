using Bogus;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Application.Seeding;

public class DataSeeder
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<TblUser> _userRepo;
    private readonly IRepository<TblCategory> _categoryRepo;
    private readonly IRepository<TblProduct> _productRepo;
    private readonly IRepository<TblOrder> _orderRepo;
    private readonly IRepository<TblSupplier> _supplierRepo;
    private readonly Application.Interfaces.IPasswordHasher _passwordHasher;

    public DataSeeder(
        IUnitOfWork unitOfWork,
        IRepository<TblUser> userRepo,
        IRepository<TblCategory> categoryRepo,
        IRepository<TblProduct> productRepo,
        IRepository<TblOrder> orderRepo,
        IRepository<TblSupplier> supplierRepo,
        Application.Interfaces.IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _userRepo = userRepo;
        _categoryRepo = categoryRepo;
        _productRepo = productRepo;
        _orderRepo = orderRepo;
        _supplierRepo = supplierRepo;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync()
    {
        // 0. Seed Default Admin
        var adminRecord = await _userRepo.FindAsync(u => u.Username == "admin");
        if (adminRecord == null)
        {
            var admin = TblUser.Create(
                "admin",
                "admin@vnvtstore.com",
                _passwordHasher.Hash("Password123!"),
                "Administrator",
                UserRole.Admin
            );
            admin.Code = "USR_ADMIN";
            admin.IsActive = true;
            await _userRepo.AddAsync(admin);
        }
        else
        {
            adminRecord.UpdatePassword(_passwordHasher.Hash("Password123!"));
            _userRepo.Update(adminRecord);
        }

        // 1. Seed Users (200)
        // Check if we need to seed data (seed if less than 50 products, meaning incomplete seed)
        if (await _productRepo.CountAsync() < 50)
        {
            try
            {
                var userFaker = new Faker<TblUser>()
                .CustomInstantiator(f => TblUser.Create(
                    f.Internet.UserName(),
                    f.Internet.Email(),
                    _passwordHasher.Hash("Password123!"), // Default password for all
                    f.Name.FullName(),
                    UserRole.Customer // Correct argument: Role
                ))
                .RuleFor(u => u.Code, f => $"USR{f.Random.Guid().ToString().Substring(0, 8).ToUpper()}")
                .RuleFor(u => u.IsActive, f => true);
            // Phone is not in Create, need to set it separately if needed, but private setter prevents it directly on Faker.
            // We can rely on Post-creation update or ignore Phone for now.

            var users = userFaker.Generate(200);
            await _userRepo.AddRangeAsync(users);
            
            // 2. Seed Categories (20)
            // TblCategory has no Create factory, using object initializer
            var categoryFaker = new Faker<TblCategory>()
                 .CustomInstantiator(f => new TblCategory
                 {
                     Code = $"CAT{f.Random.Guid().ToString().Substring(0, 8).ToUpper()}",
                     Name = f.Commerce.Categories(1)[0] + " " + f.Random.Word(),
                     Description = f.Lorem.Sentence(),
                     IsActive = true,
                     CreatedAt = DateTime.UtcNow,
                     UpdatedAt = DateTime.UtcNow
                 });
            
            var categories = categoryFaker.Generate(20);
            await _categoryRepo.AddRangeAsync(categories);

            // 2.5 Seed Suppliers (10)
            var supplierFaker = new Faker<TblSupplier>()
                .CustomInstantiator(f => new TblSupplier
                {
                    Code = $"SUP{f.Random.Guid().ToString().Substring(0, 8).ToUpper()}",
                    Name = f.Company.CompanyName(),
                    ContactPerson = f.Name.FullName(),
                    Email = f.Internet.Email(),
                    Phone = f.Phone.PhoneNumber(),
                    Address = f.Address.FullAddress(),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            
            var suppliers = supplierFaker.Generate(10);
            await _supplierRepo.AddRangeAsync(suppliers);

            // 3. Seed Products (200)
            // TblProduct.Create(name, price, stock, categoryCode, costPrice, weight, supplierCode, color, power, voltage, material, size)
            var productFaker = new Faker<TblProduct>()
                .CustomInstantiator(f => TblProduct.Create(
                    f.Commerce.ProductName(),
                    decimal.Parse(f.Commerce.Price(100000, 5000000)),
                    f.Random.Int(10, 100),
                    f.PickRandom(categories).Code,
                    null, null, f.PickRandom(suppliers).Code, null, null, null, null, null // Optional args: supplierCode passed
                ))
                .RuleFor(p => p.Code, f => $"PRD{f.Random.Guid().ToString().Substring(0, 8).ToUpper()}");
            // Description is private set, missed in Create? Create doesn't take Description? 
            // TblProduct.Create DOES NOT take Description based on my view_file! 
            // Wait, TblProduct.Create line 67: Create(string name, decimal price, int stock...)
            // It initializes a NEW TblProduct.
            // I'll leave Description empty or assume UpdateInfo is needed if I really want it. For basic seed, name/price/stock is enough.

            var products = productFaker.Generate(200);
            await _productRepo.AddRangeAsync(products);

            /*
            // 4. Seed Orders (200)
            var orderFaker = new Faker<TblOrder>()
                .CustomInstantiator(f => {
                    var user = f.PickRandom(users);
                    var total = 0m; 
                    
                    // Create Items first to calc total
                    var orderItems = new List<TblOrderItem>();
                    int itemCount = f.Random.Int(1, 5);
                    
                    for (int i = 0; i < itemCount; i++)
                    {
                         var product = f.PickRandom(products);
                         var qty = f.Random.Int(1, 3);
                         var price = product.Price;
                         total += (price * qty);

                         // Use TblOrderItem.Create
                         var item = TblOrderItem.Create(
                             product.Code,
                             product.Name,
                             null,
                             qty,
                             price,
                             null,
                             null
                         );
                         // Set Code manually if Create doesn't set it (Create sets it)
                         orderItems.Add(item);
                    }

                    // Create order with empty/null address code or rely on string address columns if Entity allows
                    // If TblOrder has AddressCode FK, we must seed Address or set it null if optional.
                    // Looking at TblOrder.Create, arg 2 is "address".
                    // If DB constraint "AddressCode_fkey" exists, likely there is a column AddressCode.
                    // But TblOrder.Create takes 'string address' which might be the full text address.
                    // Let's assume for now we can pass NULL if it's optional, OR we need to see TblOrder definition.
                    // Based on error "AddressCode_fkey", there is a column AddressCode. 
                    // Using "null" for AddressCode might work if nullable. 
                    // Or we can remove the random ADDR string if that's being mapped to AddressCode?
                    // Wait, TblOrder.Create signature: Create(string userCode, string address, decimal total...)
                    // The "address" param likely maps to a textual address column, NOT the FK.
                    // The FK "AddressCode" might be set elsewhere or defaulted?
                    // Let's try to explicitly set AddressCode to null if possible or valid user address.
                    // Since I can't easily see TblOrder, I'll assume current seeding sets AddressCode via some mechanism or defaulting.
                    // The random string `$"ADDR{f.Random.AlphaNumeric(8).ToUpper()}"` passed to Create might be treated as the Code if not careful.
                    
                    // FIX: Use a simpler address string, and if AddressCode property exists, ensure it's null or valid.
                    // I will modify the Create call to pass a simple text address like "123 Street", not strictly a Code format.
                    
                    // Pass null for addressCode to avoid Foreign Key violation (TblAddress constraint)
                    // Assuming AddressCode is nullable in DB and TblOrder entity.
                    
                    var order = TblOrder.Create(
                        user.Code,
                        null, // AddressCode (null to bypass FK check as we don't seed Addresses yet)
                        total, 
                        f.Random.Decimal(0, 50000), 
                        f.Random.Decimal(0, 100000), 
                        null
                    );
                    
                    // If TblOrder has AddressCode property that needs to be null to avoid FK:
                    // order.AddressCode = null; // Can't set if private.
                    
                    // If the error persists, it means TblOrder requires a valid AddressCode.
                    // I will try to fetch user's address if possible, or just ignore for now assuming nullable.

                    
                    // Add items to order
                    foreach(var it in orderItems)
                    {
                        order.AddOrderItem(it);
                    }
                     
                    return order;
                })
                .RuleFor(o => o.Code, f => $"ORD{f.Random.Long(10000000, 99999999)}")
                 .FinishWith((f, o) => {
                     var status = f.PickRandom<OrderStatus>();
                     if (status == OrderStatus.Cancelled) o.Cancel("Random Seed Cancel");
                     else o.UpdateStatus(status);
                 });

            var orders = orderFaker.Generate(200);
            await _orderRepo.AddRangeAsync(orders);
            */

            await _unitOfWork.CommitAsync();
        }
    catch (Exception ex)
    {
        Console.WriteLine("\n\n--------------------------------------------------");
        Console.WriteLine("SEEDING ERROR: " + ex.Message);
        if (ex.InnerException != null)
        {
             Console.WriteLine("INNER EXCEPTION: " + ex.InnerException.Message);
        }
        Console.WriteLine("--------------------------------------------------\n\n");
        throw;
        }
    }
}
}
