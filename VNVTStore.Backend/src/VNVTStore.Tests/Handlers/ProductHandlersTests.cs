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
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;

using VNVTStore.Tests.Common;

namespace VNVTStore.Tests.Handlers
{
    public class ProductHandlersTests
    {
        private readonly Mock<IRepository<TblProduct>> _mockRepo;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IDapperContext> _mockDapperContext;
        private readonly Mock<IBaseUrlService> _mockBaseUrlService;
        private readonly Mock<IFileService> _mockFileService;
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly CreateProductHandler _createHandler;
        private readonly DeleteProductHandler _deleteHandler;

        public ProductHandlersTests()
        {
            _mockRepo = new Mock<IRepository<TblProduct>>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockDapperContext = new Mock<IDapperContext>();
            _mockBaseUrlService = new Mock<IBaseUrlService>();
            _mockFileService = new Mock<IFileService>();
            _mockContext = new Mock<IApplicationDbContext>();

            _createHandler = new CreateProductHandler(
                _mockRepo.Object,
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockDapperContext.Object,
                _mockBaseUrlService.Object,
                _mockFileService.Object,
                _mockContext.Object
            );

            _deleteHandler = new DeleteProductHandler(
                _mockRepo.Object,
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockDapperContext.Object,
                _mockFileService.Object,
                _mockContext.Object
            );

            // Default Mocks
            _mockBaseUrlService.Setup(x => x.GetBaseUrl()).Returns("http://test.com");
            SetupEmptyDbSets();
        }

        private void SetupEmptyDbSets()
        {
            var mockFiles = MockDbSet(new List<TblFile>());
            var mockUnits = MockDbSet(new List<TblUnit>());
            var mockProductUnits = MockDbSet(new List<TblProductUnit>());

            _mockContext.Setup(c => c.TblFiles).Returns(mockFiles.Object);
            _mockContext.Setup(c => c.TblUnits).Returns(mockUnits.Object);
            _mockContext.Setup(c => c.TblProductUnits).Returns(mockProductUnits.Object);
        }

        private Mock<DbSet<T>> MockDbSet<T>(List<T> list) where T : class
        {
            var queryable = list.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            
            mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
              .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));
            
            return mockSet;
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
                .Returns(new ProductDto { Code = "TEST_CODE", Name = "New Product" });

            // Act
            var result = await _createHandler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Name.Should().Be("New Product");
            _mockRepo.Verify(r => r.AddAsync(It.IsAny<TblProduct>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task DeleteProduct_Success_PerformsSoftDelete()
        {
             // Arrange
            var productCode = "P001";
            var request = new DeleteCommand<TblProduct>(productCode);

            var product = TblProduct.Create("Product 1", 100, null, 10, "CAT1", null, null);
            product.Code = productCode;
            
            _mockRepo.Setup(r => r.GetByCodeAsync(productCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            _mockFileService.Setup(f => f.DeleteLinkedFilesAsync(productCode, "Product", It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success());

            // Act
            var result = await _deleteHandler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _mockRepo.Verify(r => r.Update(It.Is<TblProduct>(p => p.Code == productCode && p.ModifiedType == "Delete")), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
