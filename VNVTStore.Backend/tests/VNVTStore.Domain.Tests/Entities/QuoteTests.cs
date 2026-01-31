using System;
using System.Collections.Generic;
using VNVTStore.Domain.Entities;
using Xunit;
using FluentAssertions;

namespace VNVTStore.Domain.Tests.Entities
{
    public class QuoteTests
    {
        [Fact]
        public void Quote_ShouldHaveItemsCollection_Initialized()
        {
            var quote = new TblQuote();
            quote.TblQuoteItems.Should().NotBeNull();
            quote.TblQuoteItems.Should().BeEmpty();
        }

        [Fact]
        public void QuoteItems_ShouldCalculateLineTotal_Correctly()
        {
            // Arrange
            var item = new TblQuoteItem
            {
                Quantity = 5,
                ApprovedPrice = 100
            };

            // Act & Assert
            item.TotalLineAmount.Should().Be(500); // 5 * 100
        }

        [Fact]
        public void Quote_ShouldAllowAddingItems()
        {
            // Arrange
            var quote = new TblQuote();
            var item = new TblQuoteItem { ProductCode = "P1", Quantity = 1 };

            // Act
            quote.TblQuoteItems.Add(item);

            // Assert
            quote.TblQuoteItems.Should().HaveCount(1);
        }
    }
}
