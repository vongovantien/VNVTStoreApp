using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Linq.Expressions;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Users.Commands;
using VNVTStore.Application.Users.Handlers;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;
using Xunit;

namespace VNVTStore.Application.Tests.Handlers;

public class UserHandlersTests
{
    private readonly Mock<IRepository<TblUser>> _userRepositoryMock;
    private readonly Mock<IRepository<TblOrder>> _orderRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IDapperContext> _dapperContextMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly UserHandlers _handler;

    public UserHandlersTests()
    {
        _userRepositoryMock = new Mock<IRepository<TblUser>>();
        _orderRepositoryMock = new Mock<IRepository<TblOrder>>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _dapperContextMock = new Mock<IDapperContext>();
        _fileServiceMock = new Mock<IFileService>();

        _handler = new UserHandlers(
            _userRepositoryMock.Object,
            _orderRepositoryMock.Object,
            _passwordHasherMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _dapperContextMock.Object,
            _fileServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_CreateUser_Success_ShouldReturnUserDto()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123",
            FullName = "Test User",
            Role = "Customer",
            IsActive = true
        };
        var command = new CreateCommand<CreateUserDto, UserDto>(dto);

        _userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TblUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TblUser?)null);

        _passwordHasherMock.Setup(x => x.Hash(dto.Password)).Returns("hashed_password");

        _mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<TblUser>()))
            .Returns(new UserDto { Username = dto.Username, Email = dto.Email });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(dto.Username, result.Value?.Username);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<TblUser>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CreateUser_Conflict_ShouldReturnFailure()
    {
        // Arrange
        var dto = new CreateUserDto { Username = "existing", Email = "test@example.com" };
        var command = new CreateCommand<CreateUserDto, UserDto>(dto);

        _userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TblUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TblUser.Create("existing", "test@example.com", "hash", "Name", UserRole.Customer));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Conflict", result.Error?.Code);
        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UpdateProfile_Success_ShouldReturnUpdatedUser()
    {
        // Arrange
        var userCode = "USER001";
        var command = new UpdateProfileCommand(userCode, "New Name", "0909090909", "new@example.com", "avatar.jpg");
        
        var user = TblUser.Create("user", "old@example.com", "hash", "Old Name", UserRole.Customer);
        // Use reflection to set code as it's private set in base/entity usually or auto-generated
        user.GetType().GetProperty("Code")?.SetValue(user, userCode);

        _userRepositoryMock.Setup(x => x.GetByCodeAsync(userCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<TblUser>()))
            .Returns(new UserDto { FullName = "New Name" });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("New Name", result.Value?.FullName);
        _userRepositoryMock.Verify(x => x.Update(user), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ChangePassword_Success_ShouldReturnTrue()
    {
        // Arrange
        var userCode = "USER001";
        var currentPwd = "oldpass";
        var newPwd = "newpass";
        var command = new ChangePasswordCommand(userCode, currentPwd, newPwd);

        var user = TblUser.Create("user", "email", "hashed_old", "Name", UserRole.Customer);
        
        _userRepositoryMock.Setup(x => x.GetByCodeAsync(userCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(x => x.Verify(currentPwd, "hashed_old")).Returns(true);
        _passwordHasherMock.Setup(x => x.Hash(newPwd)).Returns("hashed_new");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _userRepositoryMock.Verify(x => x.Update(user), Times.Once);
        Assert.Equal("hashed_new", user.PasswordHash);
    }

    [Fact]
    public async Task Handle_ChangePassword_IncorrectCurrent_ShouldReturnFailure()
    {
        // Arrange
        var userCode = "USER001";
        var command = new ChangePasswordCommand(userCode, "wrong", "new");

        var user = TblUser.Create("user", "email", "hashed_old", "Name", UserRole.Customer);

        _userRepositoryMock.Setup(x => x.GetByCodeAsync(userCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(x => x.Verify("wrong", "hashed_old")).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Validation", result.Error?.Code);
    }
}
