using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using VNVTStore.Application.Suppliers.Handlers;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Tests.Helpers;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using Xunit;

namespace VNVTStore.Application.Tests.Handlers;

public class SupplierHandlersTests
{
    private readonly Mock<IRepository<TblSupplier>> _supplierRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IDapperContext> _dapperContextMock;

    public SupplierHandlersTests()
    {
        _supplierRepositoryMock = new Mock<IRepository<TblSupplier>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _dapperContextMock = new Mock<IDapperContext>();

        TestingUtils.SetupDapperMock(_dapperContextMock);
    }

    [Fact(Skip = "Dapper mocking issue in Unit Test environment")]
    public async Task Handle_GetSuppliers_ShouldReturnPagedList()
    {
        // Arrange
        var handler = new SupplierHandlers(
            _supplierRepositoryMock.Object, 
            _unitOfWorkMock.Object, 
            _mapperMock.Object, 
            _dapperContextMock.Object);

        var query = new GetPagedQuery<SupplierDto>(1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_CreateSupplier_Success_ShouldAddEntity()
    {
        // Arrange
        var dto = new CreateSupplierDto { Name = "New Sup", ContactPerson = "Contact", Phone = "090", Email = "sup@test.com" };
        var command = new CreateCommand<CreateSupplierDto, SupplierDto>(dto);
        
        var handler = new SupplierHandlers(
            _supplierRepositoryMock.Object, 
            _unitOfWorkMock.Object, 
            _mapperMock.Object, 
            _dapperContextMock.Object);

        var supplier = new TblSupplier { Code = "SUP001", Name = "New Sup" };
        _mapperMock.Setup(m => m.Map<TblSupplier>(It.IsAny<CreateSupplierDto>())).Returns(supplier);
        _mapperMock.Setup(m => m.Map<SupplierDto>(It.IsAny<TblSupplier>())).Returns(new SupplierDto { Name = "New Sup" });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _supplierRepositoryMock.Verify(x => x.AddAsync(It.IsAny<TblSupplier>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
