
using Moq;
using VNVTStore.Application.Auth.Commands;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Application.Common;
using System.Linq.Expressions;
// using VNVTStore.Application.Auth.Handlers; // Handlers might be in subfolders or same namespace. Let's assume same or subfolder. 
// Actually Handlers are likely in Handlers subfolder but namespace?
// UserHandlers was VNVTStore.Application.Users.Handlers.
// AuthHandlers likely VNVTStore.Application.Auth.Handlers.
using VNVTStore.Application.Auth.Handlers;
using AutoMapper;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Tests.Auth;

public class AuthTests
{
    private readonly Mock<IRepository<TblUser>> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;

    public AuthTests()
    {
        _userRepositoryMock = new Mock<IRepository<TblUser>>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtServiceMock = new Mock<IJwtService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var command = new LoginCommand("testuser", "password");
        var user = TblUser.Create("testuser", "test@example.com", "hashed_password", "Test User", "customer");
        var token = "token";
        var refreshToken = "refreshToken";
        
        _userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TblUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        _jwtServiceMock.Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(token);
        _jwtServiceMock.Setup(x => x.GenerateRefreshToken()).Returns(refreshToken);

        _mapperMock.Setup(x => x.Map<UserDto>(user)).Returns(new UserDto { Username = "testuser" });

        var handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtServiceMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("token", result.Value.Token);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var command = new RegisterCommand("testuser", "test@example.com", "password", "Test User");
        
        _userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TblUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TblUser?)null);

        _passwordHasherMock.Setup(x => x.Hash(It.IsAny<string>()))
            .Returns("hashed_password");
            
        _mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<TblUser>())).Returns(new UserDto { Username = "testuser" });

        var handler = new RegisterCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<TblUser>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
