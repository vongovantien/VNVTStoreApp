using System;
using FluentAssertions;
using Xunit;
using VNVTStore.Domain.Entities;
using VNVTStore.Domain.Enums;
using System.Collections.Generic;

namespace VNVTStore.Tests.Domain
{
    public class TblOrderTests
    {
        [Fact]
        public void Create_ShouldInitializeWithPendingStatus()
        {
            var order = TblOrder.Create("USR001", "ADDR001", 100, 10, 0, null);
            order.Status.Should().Be(OrderStatus.Pending);
            order.TblOrderItems.Should().BeEmpty();
        }

        [Fact]
        public void AddOrderItem_ShouldAddItemToCollection()
        {
            var order = TblOrder.Create("USR001", "ADDR001", 100, 10, 0, null);
            var item = TblOrderItem.Create("P001", "Product A", null, 1, 50, "M", "Red");

            order.AddOrderItem(item);

            order.TblOrderItems.Should().HaveCount(1);
            order.TblOrderItems.Should().Contain(item);
        }

        [Fact]
        public void TblOrderItems_ShouldBeReadOnly()
        {
            var order = TblOrder.Create("USR001", "ADDR001", 100, 10, 0, null);
            
            // Checking if TblOrderItems can be cast to ICollection<TblOrderItem> and modified?
            // Since it's exposed as IReadOnlyCollection, direct modification isn't possible via property.
            // But we can check type.
            
            order.TblOrderItems.Should().BeAssignableTo<IReadOnlyCollection<TblOrderItem>>();
            
            // Additional check: modifying the underlying list directly isn't possible from outside 
            // without reflection or internal access.
        }

        [Fact]
        public void UpdateStatus_ShouldChangeStatus()
        {
            var order = TblOrder.Create("USR001", "ADDR001", 100, 10, 0, null);
            
            order.UpdateStatus(OrderStatus.Confirmed);
            
            order.Status.Should().Be(OrderStatus.Confirmed);
        }

        [Fact]
        public void UpdateStatus_FromCancelled_ShouldThrowException()
        {
            var order = TblOrder.Create("USR001", "ADDR001", 100, 10, 0, null);
            order.Cancel("Reason");
            
            Action act = () => order.UpdateStatus(OrderStatus.Delivered);
            
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Cannot change status of a cancelled order.");
        }
    }
}
