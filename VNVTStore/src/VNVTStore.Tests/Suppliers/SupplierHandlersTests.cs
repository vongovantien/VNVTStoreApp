using AutoMapper;
using Moq;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Suppliers.Commands;
using VNVTStore.Application.Suppliers.Handlers;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using Xunit;

namespace VNVTStore.Tests.Suppliers;

public class SupplierHandlersTests
{
    private readonly Mock<IRepository<TblSupplier>> _supplierRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly SupplierHandlers _handler;

    public SupplierHandlersTests()
    {
        _supplierRepoMock = new Mock<IRepository<TblSupplier>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        
        _handler = new SupplierHandlers(
            _supplierRepoMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task CreateSupplier_Valid_ReturnsSuccess()
    {
        // Arrange
        var dto = new SupplierDto { Code = "SUP001", Name = "Test Supplier" };
        
        _supplierRepoMock.Setup(r => r.AddAsync(It.IsAny<TblSupplier>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mapperMock.Setup(m => m.Map<SupplierDto>(It.IsAny<TblSupplier>())).Returns(dto);

        // Act
        var result = await _handler.Handle(
            new CreateSupplierCommand("Test Supplier", null, null, null, null, null, null, null, null),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Test Supplier", result.Value.Name);
    }
}
