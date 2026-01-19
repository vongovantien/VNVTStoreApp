using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using System.Linq.Expressions;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Users.Commands;
using VNVTStore.Application.Users.Handlers;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Tests.Handlers;

public class UserHandlersTests
{
    private readonly Mock<IRepository<TblUser>> _mockUserRepo;
    private readonly Mock<IRepository<TblOrder>> _mockOrderRepo;
    private readonly Mock<IPasswordHasher> _mockHasher;
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IMapper> _mockMapper;
    private readonly UserHandlers _handler;

    public UserHandlersTests()
    {
        _mockUserRepo = new Mock<IRepository<TblUser>>();
        _mockOrderRepo = new Mock<IRepository<TblOrder>>();
        _mockHasher = new Mock<IPasswordHasher>();
        _mockUow = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();

        _handler = new UserHandlers(
            _mockUserRepo.Object,
            _mockOrderRepo.Object,
            _mockHasher.Object,
            _mockUow.Object,
            _mockMapper.Object
        );
    }

    [Fact]
    public async Task Delete_ShouldFail_WhenActiveOrdersExist()
    {
        _mockOrderRepo.Setup(r => r.CountAsync(It.IsAny<Expression<Func<TblOrder, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var command = new DeleteCommand<TblUser>("USER01");
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains("3 active orders", result.Error.Message);
    }

    [Fact]
    public async Task Delete_ShouldSucceed_WhenNoActiveOrders()
    {
        _mockOrderRepo.Setup(r => r.CountAsync(It.IsAny<Expression<Func<TblOrder, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var user = TblUser.Create("u1", "e1", "h1", "f1", VNVTStore.Domain.Enums.UserRole.Customer);
        user.IsActive = false; // Must be inactive to delete
        _mockUserRepo.Setup(r => r.GetByCodeAsync("USER01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new DeleteCommand<TblUser>("USER01");
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _mockUserRepo.Verify(r => r.Update(user), Times.Once);
    }

    [Fact]
    public async Task ChangePassword_ShouldFail_WhenCurrentPasswordWrong()
    {
        var user = TblUser.Create("u1", "e1", "HASH_OLD", "f1", VNVTStore.Domain.Enums.UserRole.Customer);
        _mockUserRepo.Setup(r => r.GetByCodeAsync("USER01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        
        _mockHasher.Setup(h => h.Verify("WRONG", "HASH_OLD")).Returns(false);

        var command = new ChangePasswordCommand("USER01", "WRONG", "NEW");
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Validation", result.Error.Code);
    }

    [Fact]
    public async Task ChangePassword_ShouldSucceed_WhenCurrentPasswordCorrect()
    {
        var user = TblUser.Create("u1", "e1", "HASH_OLD", "f1", VNVTStore.Domain.Enums.UserRole.Customer);
        _mockUserRepo.Setup(r => r.GetByCodeAsync("USER01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        
        _mockHasher.Setup(h => h.Verify("CORRECT", "HASH_OLD")).Returns(true);
        _mockHasher.Setup(h => h.Hash("NEW")).Returns("HASH_NEW");

        var command = new ChangePasswordCommand("USER01", "CORRECT", "NEW");
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("HASH_NEW", user.PasswordHash);
        _mockUserRepo.Verify(r => r.Update(user), Times.Once);
    }

    [Fact]
    public async Task UpdateProfile_ShouldUpdateFields()
    {
        var user = TblUser.Create("u1", "e1", "h1", "f1", VNVTStore.Domain.Enums.UserRole.Customer);
        _mockUserRepo.Setup(r => r.GetByCodeAsync("USER01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new UpdateProfileCommand("USER01", "New Name", "123456", "new@email.com");
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("New Name", user.FullName);
        Assert.Equal("123456", user.Phone);
        Assert.Equal("new@email.com", user.Email);
    }
}
