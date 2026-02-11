using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using AutoMapper;
using FluentAssertions;
using Xunit;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.SystemConfig.Queries;
using VNVTStore.Application.SystemConfig.Commands;
using VNVTStore.Domain.Entities;
using VNVTStore.Infrastructure.Persistence;
using VNVTStore.Application.Interfaces;
using VNVTStore.Tests.Common;

namespace VNVTStore.Tests.Handlers
{
    public class SystemConfigHandlersTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IMapper> _mockMapper;
        private readonly GetSystemConfigQueryHandler _getQueryHandler;
        private readonly UpdateSystemConfigCommandHandler _updateCommandHandler;

        public SystemConfigHandlersTests()
        {
            _context = TestDbContextFactory.Create();
            _mockMapper = new Mock<IMapper>();

            // Initialize Handlers
            _getQueryHandler = new GetSystemConfigQueryHandler(_context, _mockMapper.Object);
            _updateCommandHandler = new UpdateSystemConfigCommandHandler(_context, _mockMapper.Object);
        }

        /*
        [Fact]
        public async Task GetSystemConfig_ShouldReturnDto_WhenConfigExists()
        {
            // ...
        }

        [Fact]
        public async Task GetSystemConfig_ShouldReturnFailure_WhenConfigDoesNotExist()
        {
            // ...
        }

        [Fact]
        public async Task UpdateSystemConfig_ShouldCreateNew_WhenConfigDoesNotExist()
        {
            // ...
        }

        [Fact]
        public async Task UpdateSystemConfig_ShouldUpdateExisting_WhenConfigExists()
        {
            // ...
        }
        */

        public void Dispose()
        {
            TestDbContextFactory.Destroy(_context);
        }
    }
}
