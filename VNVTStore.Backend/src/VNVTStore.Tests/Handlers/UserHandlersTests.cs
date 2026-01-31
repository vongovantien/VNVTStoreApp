using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using VNVTStore.Application.Common;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Users.Commands;
using VNVTStore.Application.Users.Handlers;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Application.Users.Queries;
using VNVTStore.Application.DTOs;
using Xunit;

namespace VNVTStore.Tests.Handlers;

public class UserHandlersTests
{
    private readonly Mock<IRepository<TblUser>> _mockUserRepo;
    private readonly Mock<IRepository<TblOrder>> _mockOrderRepo;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IDapperContext> _mockDapperContext;
    private readonly UserHandlers _handler;

    public UserHandlersTests()
    {
        _mockUserRepo = new Mock<IRepository<TblUser>>();
        _mockOrderRepo = new Mock<IRepository<TblOrder>>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockDapperContext = new Mock<IDapperContext>();

        _handler = new UserHandlers(
            _mockUserRepo.Object,
            _mockOrderRepo.Object,
            _mockPasswordHasher.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockDapperContext.Object
        );
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        // Arrange
        var userCode = "U123";
        var currentPassword = "OldPassword123!";
        var newPassword = "NewPassword123!";
        var hashedOld = "hashed_old";
        var hashedNew = "hashed_new";

        var user = TblUser.Create("testuser", "test@example.com", hashedOld, "Test User", VNVTStore.Domain.Enums.UserRole.Customer);
        // Simulate existing user
        _mockUserRepo.Setup(r => r.GetByCodeAsync(userCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Simulate password validation success
        _mockPasswordHasher.Setup(h => h.Verify(currentPassword, hashedOld))
            .Returns(true);
        _mockPasswordHasher.Setup(h => h.Hash(newPassword))
            .Returns(hashedNew);

        var command = new ChangePasswordCommand(userCode, currentPassword, newPassword);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        // Verify user password was updated
        _mockUserRepo.Verify(r => r.Update(It.Is<TblUser>(u => u.PasswordHash == hashedNew)), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var userCode = "U999";
        _mockUserRepo.Setup(r => r.GetByCodeAsync(userCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TblUser)null);

        var command = new ChangePasswordCommand(userCode, "any", "any");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("NotFound", result.Error.Code); // Assuming Error.NotFound uses this naming
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnFailure_WhenCurrentPasswordIncorrect()
    {
        // Arrange
        var userCode = "U123";
        var currentPassword = "WrongPassword";
        var realHash = "real_hash";
        
        var user = TblUser.Create("test", "test@test.com", realHash, "Test", VNVTStore.Domain.Enums.UserRole.Customer);
        _mockUserRepo.Setup(r => r.GetByCodeAsync(userCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordHasher.Setup(h => h.Verify(currentPassword, realHash))
            .Returns(false);

        var command = new ChangePasswordCommand(userCode, currentPassword, "NewPass");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Validation", result.Error.Code);
    }

    [Fact]
    public async Task UpdateProfile_ShouldReturnSuccess_WhenDataIsValid()
    {
        // Arrange
        var userCode = "U123";
        var user = TblUser.Create("test", "test@test.com", "hash", "Old Name", VNVTStore.Domain.Enums.UserRole.Customer);
        _mockUserRepo.Setup(r => r.GetByCodeAsync(userCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        
        var command = new UpdateProfileCommand(userCode, "New Name", "0909000000", "new@test.com");

        _mockMapper.Setup(m => m.Map<UserDto>(It.IsAny<TblUser>()))
            .Returns(new UserDto { FullName = "New Name" });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("New Name", result.Value.FullName);
        _mockUserRepo.Verify(r => r.Update(It.Is<TblUser>(u => u.FullName == "New Name")), Times.Once);
    }

    [Fact]
    public async Task UpdateProfile_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetByCodeAsync("U999", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TblUser)null);
        
        var command = new UpdateProfileCommand("U999", "Name", "0909", "email");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("NotFound", result.Error.Code);
    }

    [Fact]
    public async Task GetUserProfile_ShouldReturnData_WhenUserExists()
    {
        // Arrange
        var userCode = "U123";
        var user = TblUser.Create("test", "test@test.com", "hash", "Test Name", VNVTStore.Domain.Enums.UserRole.Customer);
        
        _mockUserRepo.Setup(r => r.GetByCodeAsync(userCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        
        _mockMapper.Setup(m => m.Map<UserDto>(user))
            .Returns(new UserDto { Code = userCode, FullName = "Test Name" });

        var query = new GetUserProfileQuery(userCode);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(userCode, result.Value.Code);
    }

    [Fact]
    public async Task GetUserProfile_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetByCodeAsync("U999", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TblUser)null);

        var query = new GetUserProfileQuery("U999");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("NotFound", result.Error.Code);
    }
}
