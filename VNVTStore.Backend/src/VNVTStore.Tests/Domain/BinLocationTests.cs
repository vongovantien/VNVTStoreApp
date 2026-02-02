using System;
using FluentAssertions;
using Xunit;
using VNVTStore.Domain.ValueObjects;

namespace VNVTStore.Tests.Domain
{
    public class BinLocationTests
    {
        [Fact]
        public void Create_WithValidValue_ReturnsBinLocation()
        {
            var loc = BinLocation.Create("A1-01");
            loc.Value.Should().Be("A1-01");
        }

        [Fact]
        public void Create_WithEmptyValue_ThrowsArgumentException()
        {
            Action act = () => BinLocation.Create("");
            act.Should().Throw<ArgumentException>()
               .WithMessage("*Bin location cannot be empty*");
        }

        [Fact]
        public void Create_WithTooLongValue_ThrowsArgumentException()
        {
            var longValue = new string('A', 21);
            Action act = () => BinLocation.Create(longValue);
            act.Should().Throw<ArgumentException>()
               .WithMessage("*Bin location cannot exceed 20 characters*");
        }

        [Fact]
        public void ImplicitConversion_ToString_Works()
        {
            var loc = BinLocation.Create("B2-05");
            string str = loc;
            str.Should().Be("B2-05");
        }

        [Fact]
        public void ImplicitConversion_FromString_Works()
        {
            BinLocation loc = "C3-10";
            loc.Value.Should().Be("C3-10");
        }

        [Fact]
        public void Equality_Check_ShouldWork()
        {
            var loc1 = BinLocation.Create("D4");
            var loc2 = BinLocation.Create("D4");
            var loc3 = BinLocation.Create("E5");

            loc1.Should().Be(loc2);
            loc1.Should().NotBe(loc3);
        }
    }
}
