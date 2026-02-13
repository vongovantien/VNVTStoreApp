using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using AutoMapper;
using Moq;
using Microsoft.EntityFrameworkCore;
using Xunit;
using VNVTStore.Application.Common;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.Users.Commands;
using VNVTStore.Application.Users.Handlers;
using VNVTStore.Application.Users.Queries;
using VNVTStore.Application.DTOs;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Enums;
using VNVTStore.Infrastructure.Persistence;
using VNVTStore.Infrastructure.Persistence.Repositories;
using VNVTStore.Tests.Common;

namespace VNVTStore.Tests.Handlers;

public class UserHandlersTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IRepository<TblUser> _repository;
    private readonly IRepository<TblOrder> _orderRepository;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IDapperContext> _mockDapperContext;
    private readonly Mock<IFileService> _mockFileService;
    private readonly UserHandlers _handler;

    public UserHandlersTests()
    {
        _context = TestDbContextFactory.Create();
        _repository = new Repository<TblUser>(_context);
        _orderRepository = new Repository<TblOrder>(_context);
        
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockDapperContext = new Mock<IDapperContext>();
        _mockFileService = new Mock<IFileService>();

        _handler = new UserHandlers(
            _repository,
            _orderRepository,
            _mockPasswordHasher.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockDapperContext.Object,
            _mockFileService.Object
        );
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
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

        var user = TblUser.Create("testuser", "test@example.com", hashedOld, "Test User", UserRole.Customer);
        user.Code = userCode;
        await _context.TblUsers.AddAsync(user);
        await _context.SaveChangesAsync();

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
        
        // Verify user password was updated in DB
        var updatedUser = await _context.TblUsers.FirstAsync(u => u.Code == userCode);
        Assert.Equal(hashedNew, updatedUser.PasswordHash);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var userCode = "U999";

        var command = new ChangePasswordCommand(userCode, "any", "any");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("NotFound", result.Error.Code);
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnFailure_WhenCurrentPasswordIncorrect()
    {
        // Arrange
        var userCode = "U123";
        var currentPassword = "WrongPassword";
        var realHash = "real_hash";
        
        var user = TblUser.Create("test", "test@test.com", realHash, "Test", UserRole.Customer);
        user.Code = userCode;
        await _context.TblUsers.AddAsync(user);
        await _context.SaveChangesAsync();

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
        var user = TblUser.Create("test", "test@test.com", "hash", "Old Name", UserRole.Customer);
        user.Code = userCode;
        await _context.TblUsers.AddAsync(user);
        await _context.SaveChangesAsync();
        
        var command = new UpdateProfileCommand(userCode, "New Name", "0909000000", "new@test.com", null);

        _mockMapper.Setup(m => m.Map<UserDto>(It.IsAny<TblUser>()))
            .Returns(new UserDto { FullName = "New Name" });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("New Name", result.Value.FullName);
        
        // Verify DB update
        var updatedUser = await _context.TblUsers.FirstAsync(u => u.Code == userCode);
        Assert.Equal("New Name", updatedUser.FullName);
    }

    [Fact]
    public async Task UpdateProfile_ShouldReturnFailure_WhenUserNotFound()
    {
        var command = new UpdateProfileCommand("U999", "Name", "0909", "email", null);

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
        var user = TblUser.Create("test", "test@test.com", "hash", "Test Name", UserRole.Customer);
        user.Code = userCode;
        await _context.TblUsers.AddAsync(user);
        await _context.SaveChangesAsync();
        
        _mockMapper.Setup(m => m.Map<UserDto>(It.IsAny<TblUser>()))
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
        var query = new GetUserProfileQuery("U999");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("NotFound", result.Error.Code);
    }
}
