using Moq;
using AutoMapper;
using Xunit;
using System.Collections.Generic;
using VNVTStore.Application.Quotes.Handlers;
using VNVTStore.Domain.Interfaces;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Common;
using VNVTStore.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using VNVTStore.Application.Quotes.Queries;
using MediatR;

namespace VNVTStore.Application.Tests.Handlers;

public class QuoteHandlersTests
{
    private readonly Mock<IRepository<TblQuote>> _quoteRepoMock;
    private readonly Mock<IRepository<TblProduct>> _productRepoMock;
    private readonly Mock<IRepository<TblUser>> _userRepoMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly QuoteHandlers _handler;

    public QuoteHandlersTests()
    {
        _quoteRepoMock = new Mock<IRepository<TblQuote>>();
        _productRepoMock = new Mock<IRepository<TblProduct>>();
        _userRepoMock = new Mock<IRepository<TblUser>>();
        _emailServiceMock = new Mock<IEmailService>();
        _currentUserMock = new Mock<ICurrentUser>();
        _uowMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();

        _handler = new QuoteHandlers(
            _quoteRepoMock.Object,
            _productRepoMock.Object,
            _userRepoMock.Object,
            _emailServiceMock.Object,
            _currentUserMock.Object,
            _uowMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_CreateQuote_ShouldSendEmailToAdmins()
    {
        // Arrange
        var request = new CreateCommand<CreateQuoteDto, QuoteDto>(new CreateQuoteDto
        {
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            Items = new List<CreateQuoteItemDto> { new CreateQuoteItemDto { ProductCode = "P1", Quantity = 1 } }
        });

        _currentUserMock.Setup(x => x.UserCode).Returns("U1");
        _productRepoMock.Setup(x => x.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TblProduct.Create("Test Product", 10m, null, 100, null, null, null));

        var admins = new List<TblUser>
        {
            TblUser.Create("admin", "admin@example.com", "hash", "Admin User", UserRole.Admin)
        }.AsQueryable();

        _userRepoMock.Setup(x => x.AsQueryable()).Returns(admins);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _emailServiceMock.Verify(x => x.SendEmailAsync(
            "admin@example.com",
            It.Is<string>(s => s.Contains("[New Quote Request]")),
            It.IsAny<string>(),
            true), Times.Once);

        _uowMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _quoteRepoMock.Verify(x => x.AddAsync(It.IsAny<TblQuote>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CreateQuote_WhenNoAdmins_ShouldNotSendEmail()
    {
        // Arrange
        var request = new CreateCommand<CreateQuoteDto, QuoteDto>(new CreateQuoteDto
        {
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            Items = new List<CreateQuoteItemDto> { new CreateQuoteItemDto { ProductCode = "P1", Quantity = 1 } }
        });

        _currentUserMock.Setup(x => x.UserCode).Returns("U1");
        _productRepoMock.Setup(x => x.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TblProduct.Create("Test Product", 10m, null, 100, null, null, null));

        var noAdmins = new List<TblUser>().AsQueryable();

        _userRepoMock.Setup(x => x.AsQueryable()).Returns(noAdmins);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _emailServiceMock.Verify(x => x.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<bool>()), Times.Never);
    }
}
