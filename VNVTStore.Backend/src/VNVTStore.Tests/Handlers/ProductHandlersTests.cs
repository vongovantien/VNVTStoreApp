using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Products.Commands;
using VNVTStore.Application.Products.Handlers;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Infrastructure.Persistence;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using VNVTStore.Application.Common.Models;

using VNVTStore.Tests.Common;

namespace VNVTStore.Tests.Handlers
{
    public class ProductHandlersTests : IDisposable
    {
        private readonly Mock<IRepository<TblProduct>> _mockRepo;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork; // Keep for now as handlers might use strict UoW pattern, or we can mock it to do nothing/proxy
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IDapperContext> _mockDapperContext;
        private readonly Mock<IBaseUrlService> _mockBaseUrlService;
        private readonly Mock<IFileService> _mockFileService;
        private readonly ApplicationDbContext _context; // Real In-Memory Context
        private readonly Mock<IImageUploadService> _mockUploadService;
        private readonly CreateProductHandler _createHandler;
        private readonly DeleteProductHandler _deleteHandler;

        public ProductHandlersTests()
        {
            _context = TestDbContextFactory.Create();

            _mockRepo = new Mock<IRepository<TblProduct>>();
            // Setup Repo to use the real DbSet for Add/Update if possible, or just Verify calls.
            // For simple handlers, we can stick to mocks for Repo if they just call AddAsync.
            // But better to use the real Context-backed repository or just mock the Repo's behavior 
            // to persist to the in-memory DB if we want integration-style testing.
            // However, handlers usually take IRepository. Let's start by mocking Repo behavior to write to context.
            
            _mockRepo.Setup(r => r.AddAsync(It.IsAny<TblProduct>(), It.IsAny<CancellationToken>()))
                .Callback<TblProduct, CancellationToken>((p, c) => _context.TblProducts.Add(p))
                .Returns(Task.CompletedTask);
            
            _mockRepo.Setup(r => r.Update(It.IsAny<TblProduct>()))
                .Callback<TblProduct>(p => _context.TblProducts.Update(p));

            _mockRepo.Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns<string, CancellationToken>(async (code, c) => 
                     await _context.TblProducts.FirstOrDefaultAsync(p => p.Code == code, c));

            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(async (CancellationToken c) => await _context.SaveChangesAsync(c));
            _mockUnitOfWork.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(async (CancellationToken c) => await _context.SaveChangesAsync(c));

            _mockMapper = new Mock<IMapper>();
            _mockDapperContext = new Mock<IDapperContext>();
            _mockBaseUrlService = new Mock<IBaseUrlService>();
            _mockFileService = new Mock<IFileService>();
            _mockUploadService = new Mock<IImageUploadService>();

            _createHandler = new CreateProductHandler(
                _mockRepo.Object,
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockDapperContext.Object,
                _mockBaseUrlService.Object,
                _mockFileService.Object,
                _context // Use real context here
            );

            _deleteHandler = new DeleteProductHandler(
                _mockRepo.Object,
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockDapperContext.Object,
                _mockFileService.Object,
                _context // Use real context here
            );

            // Default Mocks
            _mockBaseUrlService.Setup(x => x.GetBaseUrl()).Returns("http://test.com");
            _mockFileService.Setup(x => x.DeleteLinkedFilesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());
                
            SeedDatabase();
        }
        
        private void SeedDatabase()
        {
            var category = new TblCategory { Code = "CAT1", Name = "Category 1", IsActive = true };
            if (!_context.TblCategories.Any(c => c.Code == category.Code))
            {
                _context.TblCategories.Add(category);
            }

            var brand = new TblBrand { Code = "BRAND1", Name = "Brand 1", IsActive = true };
             if (!_context.TblBrands.Any(b => b.Code == brand.Code))
            {
                _context.TblBrands.Add(brand);
            }

            var supplier = new TblSupplier { Code = "SUP1", Name = "Supplier 1", IsActive = true };
             if (!_context.TblSuppliers.Any(s => s.Code == supplier.Code))
            {
                _context.TblSuppliers.Add(supplier);
            }
            
            var unit = new TblUnit { Code = "UNIT1", Name = "Piece", IsActive = true };
             if (!_context.TblUnits.Any(u => u.Code == unit.Code))
            {
                _context.TblUnits.Add(unit);
            }
            
            _context.SaveChanges();
        }

        [Fact]
        public async Task CreateProduct_Success_ReturnsDto()
        {
            // Arrange
            var createDto = new CreateProductDto 
            { 
                Name = "New Product", 
                Price = 100, 
                StockQuantity = 10,
                CategoryCode = "CAT1"
            };
            var request = new CreateCommand<CreateProductDto, ProductDto>(createDto);
            
            _mockMapper.Setup(m => m.Map<ProductDto>(It.IsAny<TblProduct>()))
                .Returns((TblProduct source) => new ProductDto { Code = source.Code, Name = source.Name });

            // Act
            var result = await _createHandler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Name.Should().Be("New Product");
            
            // Verify DB State
            var dbProduct = await _context.TblProducts.FirstOrDefaultAsync(p => p.Name == "New Product");
            dbProduct.Should().NotBeNull();
            dbProduct.Price.Should().Be(100);
        }
        
        [Fact]
        public async Task DeleteProduct_Success_PerformsSoftDelete()
        {
             // Arrange
            var productCode = "P001";
            var request = new DeleteCommand<TblProduct>(productCode);

            var product = TblProduct.Create("Product 1", 100, null, 10, "CAT1", null, null);
            product.Code = productCode;
            product.IsActive = false; // Must be inactive to delete
            _context.TblProducts.Add(product);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _deleteHandler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            
            var deletedProduct = await _context.TblProducts.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Code == productCode);
            deletedProduct.Should().NotBeNull();
            deletedProduct.ModifiedType.Should().Be("Delete");
        }

        public void Dispose()
        {
            TestDbContextFactory.Destroy(_context);
        }

        [Fact]
        public async Task CreateProduct_WithImage_Success()
        {
            // Arrange
            var createDto = new CreateProductDto 
            { 
                Name = "Product With Image", 
                Price = 100, 
                StockQuantity = 10,
                CategoryCode = "CAT1",
                Images = new List<string> { "data:image/png;base64,test" }
            };
            var request = new CreateCommand<CreateProductDto, ProductDto>(createDto);
            
            var uploadedPath = "products/newimage.png";
            _mockUploadService.Setup(s => s.UploadBase64ImagesAsync(It.IsAny<List<(string, string)>>(), "products"))
                .ReturnsAsync(Result.Success<IEnumerable<FileDto>>(new List<FileDto> { new FileDto { Url = uploadedPath, Path = uploadedPath } }));

            _mockMapper.Setup(m => m.Map<ProductDto>(It.IsAny<TblProduct>()))
                .Returns((TblProduct source) => new ProductDto { Code = source.Code, Name = source.Name });

            // Create a mock file for linking
            var fileEntity = TblFile.Create("newimage.png", "newimage.png", ".png", "image/png", 100, uploadedPath);
            _context.TblFiles.Add(fileEntity);
            await _context.SaveChangesAsync();

            // Act
            var result = await _createHandler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _mockUploadService.Verify(s => s.UploadBase64ImagesAsync(It.IsAny<List<(string, string)>>(), "products"), Times.Once);
            
            var dbProduct = await _context.TblProducts.FirstOrDefaultAsync(p => p.Name == "Product With Image");
            dbProduct.Should().NotBeNull();
            
            // Verify linking happened on the file entity itself
            var linkedFile = await _context.TblFiles.FirstOrDefaultAsync(f => f.Url == uploadedPath);
            linkedFile!.MasterCode.Should().Be(dbProduct!.Code);
            linkedFile.MasterType.Should().Be("Product");
        }
    }
}
