using AutoMapper;
using Moq;
using VNVTStore.Application.Addresses.Commands;
using VNVTStore.Application.Addresses.Handlers;
using VNVTStore.Application.Addresses.Queries;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using Xunit;

namespace VNVTStore.Tests.Addresses;

public class AddressHandlersTests
{
    private readonly Mock<IRepository<TblAddress>> _addressRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly AddressHandlers _handler;

    public AddressHandlersTests()
    {
        _addressRepoMock = new Mock<IRepository<TblAddress>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        
        _handler = new AddressHandlers(
            _addressRepoMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task CreateAddress_ValidData_ReturnsSuccess()
    {
        // Arrange
        var userCode = "USR001";
        var addressDto = new AddressDto { Code = "ADR001", AddressLine = "123 Test St" };

        _addressRepoMock.Setup(r => r.FindAllAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<TblAddress, bool>>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TblAddress>());
        _addressRepoMock.Setup(r => r.AddAsync(It.IsAny<TblAddress>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mapperMock.Setup(m => m.Map<AddressDto>(It.IsAny<TblAddress>())).Returns(addressDto);

        // Act
        var result = await _handler.Handle(
            new CreateAddressCommand(userCode, "123 Test St", "HCM", "HCM", "70000", "Vietnam", true),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("123 Test St", result.Value.AddressLine);
    }

    [Fact]
    public async Task DeleteAddress_NotFound_ReturnsFailure()
    {
        // Arrange
        var addressCode = "ADR999";
        var userCode = "USR001";

        _addressRepoMock.Setup(r => r.GetByCodeAsync(addressCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TblAddress?)null);

        // Act
        var result = await _handler.Handle(
            new DeleteAddressCommand(addressCode, userCode), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteAddress_NotOwner_ReturnsForbidden()
    {
        // Arrange
        var addressCode = "ADR001";
        var userCode = "USR001";
        var address = new TblAddress { Code = addressCode, UserCode = "USR002", AddressLine = "Test" };

        _addressRepoMock.Setup(r => r.GetByCodeAsync(addressCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(address);

        // Act
        var result = await _handler.Handle(
            new DeleteAddressCommand(addressCode, userCode), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Forbidden", result.Error!.Code, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SetDefaultAddress_Success()
    {
        // Arrange
        var addressCode = "ADR001";
        var userCode = "USR001";
        var address = new TblAddress { Code = addressCode, UserCode = userCode, AddressLine = "Test", IsDefault = false };

        _addressRepoMock.Setup(r => r.GetByCodeAsync(addressCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(address);
        _addressRepoMock.Setup(r => r.FindAllAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<TblAddress, bool>>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TblAddress>());
        _unitOfWorkMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(
            new SetDefaultAddressCommand(addressCode, userCode), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.True(address.IsDefault);
    }
}
