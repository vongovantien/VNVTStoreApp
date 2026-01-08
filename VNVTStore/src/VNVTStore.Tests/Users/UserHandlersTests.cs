using AutoMapper;
using Moq;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Users.Commands;
using VNVTStore.Application.Users.Handlers;
using VNVTStore.Application.Users.Queries;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using Xunit;

namespace VNVTStore.Tests.Users;

public class UserHandlersTests
{
    private readonly Mock<IRepository<TblUser>> _userRepoMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly UserHandlers _handler;

    public UserHandlersTests()
    {
        _userRepoMock = new Mock<IRepository<TblUser>>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        
        _handler = new UserHandlers(
            _userRepoMock.Object,
            _passwordHasherMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task GetUserProfile_UserExists_ReturnsUserDto()
    {
        // Arrange
        var userCode = "USR001";
        var user = new TblUser { Code = userCode, Username = "testuser", Email = "test@test.com" };
        var userDto = new UserDto { Code = userCode, Username = "testuser", Email = "test@test.com" };

        _userRepoMock.Setup(r => r.GetByCodeAsync(userCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mapperMock.Setup(m => m.Map<UserDto>(user)).Returns(userDto);

        // Act
        var result = await _handler.Handle(new GetUserProfileQuery(userCode), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("testuser", result.Value.Username);
    }

    [Fact]
    public async Task GetUserProfile_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var userCode = "USR999";

        _userRepoMock.Setup(r => r.GetByCodeAsync(userCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TblUser?)null);

        // Act
        var result = await _handler.Handle(new GetUserProfileQuery(userCode), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateProfile_ValidData_ReturnsUpdatedUser()
    {
        // Arrange
        var userCode = "USR001";
        var user = new TblUser { Code = userCode, Username = "testuser", Email = "old@test.com", FullName = "Old Name" };
        var userDto = new UserDto { Code = userCode, Email = "new@test.com", FullName = "New Name" };

        _userRepoMock.Setup(r => r.GetByCodeAsync(userCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepoMock.Setup(r => r.FindAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<TblUser, bool>>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((TblUser?)null); // No duplicate email
        _unitOfWorkMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mapperMock.Setup(m => m.Map<UserDto>(It.IsAny<TblUser>())).Returns(userDto);

        // Act
        var result = await _handler.Handle(
            new UpdateProfileCommand(userCode, "New Name", null, "new@test.com"), 
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("New Name", user.FullName);
        Assert.Equal("new@test.com", user.Email);
    }

    [Fact]
    public async Task ChangePassword_WrongCurrentPassword_ReturnsFailure()
    {
        // Arrange
        var userCode = "USR001";
        var user = new TblUser { Code = userCode, Username = "testuser", PasswordHash = "hashedOldPassword" };

        _userRepoMock.Setup(r => r.GetByCodeAsync(userCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.Verify("wrongPassword", "hashedOldPassword"))
            .Returns(false);

        // Act
        var result = await _handler.Handle(
            new ChangePasswordCommand(userCode, "wrongPassword", "newPassword"), 
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("incorrect", result.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ChangePassword_ValidCurrentPassword_ReturnsSuccess()
    {
        // Arrange
        var userCode = "USR001";
        var user = new TblUser { Code = userCode, Username = "testuser", PasswordHash = "hashedOldPassword" };

        _userRepoMock.Setup(r => r.GetByCodeAsync(userCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.Verify("correctPassword", "hashedOldPassword"))
            .Returns(true);
        _passwordHasherMock.Setup(h => h.Hash("newPassword"))
            .Returns("hashedNewPassword");
        _unitOfWorkMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(
            new ChangePasswordCommand(userCode, "correctPassword", "newPassword"), 
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.Equal("hashedNewPassword", user.PasswordHash);
    }
}
