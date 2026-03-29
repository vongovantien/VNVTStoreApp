using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using VNVTStore.Application.Auth.Commands;
using VNVTStore.Application.Auth.Handlers;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using VNVTStore.Domain.Interfaces;
using Xunit;

namespace VNVTStore.Application.Tests.Handlers;

public class AuthHandlersTests
{
    private readonly Mock<IRepository<TblUser>> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ICurrentUser> _currentUserServiceMock;
    private readonly Mock<IDapperContext> _dapperContextMock;
    private readonly Mock<Microsoft.Extensions.Configuration.IConfiguration> _configurationMock;
    private readonly Mock<ISecretConfigurationService> _secretConfigMock;

    public AuthHandlersTests()
    {
        _userRepositoryMock = new Mock<IRepository<TblUser>>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtServiceMock = new Mock<IJwtService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _emailServiceMock = new Mock<IEmailService>();
        _currentUserServiceMock = new Mock<ICurrentUser>();
        _dapperContextMock = new Mock<IDapperContext>();
        _configurationMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        _secretConfigMock = new Mock<ISecretConfigurationService>();
    }

    [Fact]
    public async Task Handle_Register_Success_ShouldReturnUserDto()
    {
        // Arrange
        var command = new RegisterCommand("newuser", "test@example.com", "Password@123", "New User");
        var handler = new RegisterCommandHandler(
            _userRepositoryMock.Object, 
            _passwordHasherMock.Object, 
            _unitOfWorkMock.Object, 
            _mapperMock.Object, 
            _emailServiceMock.Object,
            _configurationMock.Object,
            _secretConfigMock.Object
        );

        _userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TblUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TblUser)null); // User does not exist

        _passwordHasherMock.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashed_password");
        _mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<TblUser>())).Returns(new UserDto { Email = "test@example.com" });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("test@example.com", result.Value!.Email);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<TblUser>(), It.IsAny<CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Register_Conflict_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterCommand("existing", "test@example.com", "Password@123", "Name");
        var handler = new RegisterCommandHandler(
            _userRepositoryMock.Object, 
            _passwordHasherMock.Object, 
            _unitOfWorkMock.Object, 
            _mapperMock.Object, 
            _emailServiceMock.Object,
            _configurationMock.Object,
            _secretConfigMock.Object
        );

        _userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TblUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TblUser.Create("existing", "other@example.com", "hash", "Name", UserRole.Customer));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Conflict", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_Login_Success_ShouldReturnToken()
    {
        // Arrange
        var command = new LoginCommand("user", "Password@123");
        var handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtServiceMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object
        );

        var existingUser = TblUser.Create("user", "email", "hashed_password", "Name", UserRole.Customer);
        var users = new List<TblUser> { existingUser };
        var mockDbSet = CreateMockDbSet(users);
        
        _userRepositoryMock.Setup(x => x.Where(It.IsAny<Expression<Func<TblUser, bool>>>()))
            .Returns(mockDbSet.Object);
        
        _passwordHasherMock.Setup(x => x.Verify("Password@123", "hashed_password")).Returns(true);
        _jwtServiceMock.Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UserRole>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>())).Returns("token");
        _jwtServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh_token");
        _mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<TblUser>())).Returns(new UserDto { Username = "user" });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("token", result.Value!.Token);
        _userRepositoryMock.Verify(x => x.Update(It.IsAny<TblUser>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_Login_InvalidCredentials_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginCommand("user", "wrong_password");
        var handler = new LoginCommandHandler(
             _userRepositoryMock.Object,
             _passwordHasherMock.Object,
             _jwtServiceMock.Object,
             _unitOfWorkMock.Object,
             _mapperMock.Object
         );

        var existingUser = TblUser.Create("user", "email", "hashed_password", "Name", UserRole.Customer);
        var users = new List<TblUser> { existingUser };
        var mockDbSet = CreateMockDbSet(users);
        
        _userRepositoryMock.Setup(x => x.Where(It.IsAny<Expression<Func<TblUser, bool>>>()))
            .Returns(mockDbSet.Object);

        _passwordHasherMock.Setup(x => x.Verify("wrong_password", "hashed_password")).Returns(false);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Validation", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_Impersonate_Success_ShouldReturnToken()
    {
        // Arrange
        var adminCode = "ADMIN001";
        var targetCode = "USER001";
        var command = new ImpersonateCommand(targetCode);
        var handler = new ImpersonateCommandHandler(
            _userRepositoryMock.Object,
            _jwtServiceMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _currentUserServiceMock.Object
        );

        var adminUser = TblUser.Create("admin", "admin@vnvt.com", "hash", "Admin", UserRole.Admin);
        adminUser.GetType().GetProperty("Code")?.SetValue(adminUser, adminCode);

        var targetUser = TblUser.Create("customer", "customer@vnvt.com", "hash", "Customer", UserRole.Customer);
        targetUser.GetType().GetProperty("Code")?.SetValue(targetUser, targetCode);

        _currentUserServiceMock.Setup(x => x.UserCode).Returns(adminCode);
        
        // FindAsync for admin role check
        _userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TblUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminUser);

        // Where for target user
        var users = new List<TblUser> { targetUser };
        _userRepositoryMock.Setup(x => x.Where(It.IsAny<Expression<Func<TblUser, bool>>>()))
            .Returns(CreateMockDbSet(users).Object);

        _jwtServiceMock.Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<UserRole>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>())).Returns("impersonated_token");
        _jwtServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("new_refresh_token");
        _mapperMock.Setup(x => x.Map<UserDto>(It.IsAny<TblUser>())).Returns(new UserDto { Code = targetCode });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("impersonated_token", result.Value!.Token);
        _userRepositoryMock.Verify(x => x.Update(targetUser), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Impersonate_Forbidden_WhenNotAdmin_ShouldReturnFailure()
    {
        // Arrange
        var nonAdminCode = "USER002";
        var command = new ImpersonateCommand("USER001");
        var handler = new ImpersonateCommandHandler(_userRepositoryMock.Object, _jwtServiceMock.Object, _unitOfWorkMock.Object, _mapperMock.Object, _currentUserServiceMock.Object);

        var nonAdminUser = TblUser.Create("user", "user@vnvt.com", "hash", "User", UserRole.Customer);
        _currentUserServiceMock.Setup(x => x.UserCode).Returns(nonAdminCode);
        _userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TblUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(nonAdminUser);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Forbidden", result.Error!.Code);
    }

    [Fact]
    public async Task Handle_Impersonate_Forbidden_ImpersonatingAdmin_ShouldReturnFailure()
    {
        // Arrange
        var adminCode = "ADMIN001";
        var targetAdminCode = "ADMIN002";
        var command = new ImpersonateCommand(targetAdminCode);
        var handler = new ImpersonateCommandHandler(_userRepositoryMock.Object, _jwtServiceMock.Object, _unitOfWorkMock.Object, _mapperMock.Object, _currentUserServiceMock.Object);

        var adminUser = TblUser.Create("admin", "admin@vnvt.com", "hash", "Admin", UserRole.Admin);
        var targetAdmin = TblUser.Create("target_admin", "admin2@vnvt.com", "hash", "Admin 2", UserRole.Admin);

        _currentUserServiceMock.Setup(x => x.UserCode).Returns(adminCode);
        _userRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<TblUser, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminUser);

        var users = new List<TblUser> { targetAdmin };
        _userRepositoryMock.Setup(x => x.Where(It.IsAny<Expression<Func<TblUser, bool>>>()))
            .Returns(CreateMockDbSet(users).Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Validation", result.Error!.Code);
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> sourceList) where T : class
    {
        var queryable = sourceList.AsQueryable();
        var dbSetMock = new Mock<DbSet<T>>();

        dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
        
        dbSetMock.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

        return dbSetMock;
    }

    private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;
        public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;
        public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TEntity>(expression);
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);
        public object? Execute(Expression expression) => _inner.Execute(expression);
        public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);
        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult);
            var expectedResultType = resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Task<>) 
                ? resultType.GetGenericArguments()[0] 
                : resultType;
                
            var executionResult = _inner.Execute(expression);
            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(expectedResultType).Invoke(null, new[] { executionResult })!;
        }
    }
    
    private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(Expression expression) : base(expression) { }
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }
    
    private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;
        public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
        public T Current => _inner.Current;
        public ValueTask DisposeAsync() { _inner.Dispose(); return ValueTask.CompletedTask; }
        public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());
    }
}
