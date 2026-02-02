using System;
using FluentAssertions;
using Xunit;
using VNVTStore.Domain.Common;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Tests.Domain
{
    public class SmartEnumTests
    {
        [Fact]
        public void FromValue_WithValidValue_ReturnsCorrectEnum()
        {
            var status = OrderStatus.FromValue(0);
            status.Should().Be(OrderStatus.Pending);
            status.Name.Should().Be("Pending");
        }

        [Fact]
        public void FromValue_WithInvalidValue_ReturnsNull()
        {
            var status = OrderStatus.FromValue(999);
            status.Should().BeNull();
        }

        [Fact]
        public void FromName_WithValidName_ReturnsCorrectEnum()
        {
            var status = OrderStatus.FromName("Shipped");
            status.Should().Be(OrderStatus.Shipped);
            status.Value.Should().Be(4);
        }

        [Fact]
        public void FromName_WithValidName_CaseInsensitive_ReturnsCorrectEnum()
        {
            var status = OrderStatus.FromName("shipped");
            status.Should().Be(OrderStatus.Shipped);
        }

        [Fact]
        public void FromName_WithInvalidName_ReturnsNull()
        {
            var status = OrderStatus.FromName("InvalidStatus");
            status.Should().BeNull();
        }

        [Fact]
        public void Equality_Check_ShouldWorkCorrectly()
        {
            var status1 = OrderStatus.Delivered;
            var status2 = OrderStatus.FromValue(5);

            status1.Should().Be(status2);
            (status1 == status2).Should().BeTrue();
            (status1 != OrderStatus.Pending).Should().BeTrue();
        }
    }
}
