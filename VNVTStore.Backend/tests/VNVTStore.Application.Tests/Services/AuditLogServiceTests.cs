using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Infrastructure.Persistence;
using VNVTStore.Infrastructure.Services;
using Xunit;

namespace VNVTStore.Application.Tests.Services;

public class AuditLogServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly AuditLogService _auditLogService;

    public AuditLogServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _currentUserMock = new Mock<ICurrentUser>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        // Setup default mocks
        _currentUserMock.Setup(x => x.UserCode).Returns("USER001");
        
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

        _auditLogService = new AuditLogService(_context, _currentUserMock.Object, _httpContextAccessorMock.Object);
    }

    [Fact]
    public async Task LogAsync_ShouldCreateAuditLog_WithCorrectDetails()
    {
        // Act
        await _auditLogService.LogAsync("TEST_ACTION", "TARGET_001", "Test Details");

        // Assert
        var log = await _context.TblAuditLogs.FirstOrDefaultAsync();
        log.Should().NotBeNull();
        log!.Action.Should().Be("TEST_ACTION");
        log!.Target.Should().Be("TARGET_001");
        log!.Detail.Should().Be("Test Details");
        log!.UserCode.Should().Be("USER001");
        log!.IpAddress.Should().Be("127.0.0.1");
    }

    [Fact]
    public async Task GetLogsAsync_ShouldFilterByAction()
    {
        // Arrange
        _context.TblAuditLogs.AddRange(
            new TblAuditLog { Code = "L1", Action = "LOGIN", CreatedAt = DateTime.UtcNow },
            new TblAuditLog { Code = "L2", Action = "LOGOUT", CreatedAt = DateTime.UtcNow },
            new TblAuditLog { Code = "L3", Action = "UPDATE", CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        var paramsObj = new SearchParams { PageIndex = 1, PageSize = 10, Searching = "LOGIN" };

        // Act
        var result = await _auditLogService.GetLogsAsync(paramsObj);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Action.Should().Be("LOGIN");
    }

    [Fact]
    public async Task GetLogsAsync_ShouldReturnPagedResults()
    {
        // Arrange
        for (int i = 0; i < 15; i++)
        {
            _context.TblAuditLogs.Add(new TblAuditLog 
            { 
                Code = $"L{i}", 
                Action = $"ACTION_{i}", 
                CreatedAt = DateTime.UtcNow.AddMinutes(-i) // Newer first (default sort)
            });
        }
        await _context.SaveChangesAsync();

        var paramsObj = new SearchParams { PageIndex = 2, PageSize = 5 };

        // Act
        var result = await _auditLogService.GetLogsAsync(paramsObj);

        // Assert
        result.TotalItems.Should().Be(15);
        result.Items.Should().HaveCount(5);
        result.PageIndex.Should().Be(2);
    }
}
