using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vnvt_back_end.Application.Interfaces;

namespace vnvt_back_end.Application.DTOs
{
    public class DTOs
    {

        //public class UserProfileDto
        //{
        //    public string Username { get; set; }
        //    public string Email { get; set; }
        //    public string FullName { get; set; }
        //    // Add other profile properties as needed
        //}   //public class UserProfileDto
        //{
        //    public string Username { get; set; }
        //    public string Email { get; set; }
        //    public string FullName { get; set; }
        //    // Add other profile properties as needed
        //}

        public class GenericDto<TKey> : IBaseDto
        {
            public int Id { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime UpdatedDate { get; set; } = DateTime.Now;
        }
        public class ProductImageDto : GenericDto<int>
        {
            public string ImageUrl { get; set; }
            public int ProductId { get; set; }
        }

        public class ProductDto : GenericDto<int>
        {
            public string Name { get; set; }
            public string CategoryName { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public int CategoryId { get; set; }
            public int StockQuantity { get; set; }
            public List<ProductImageDto> ProductImages { get; set; }
        }

        public class CategoryDto : GenericDto<int>
        {
            public string Name { get; set; }
            public string Description { get; set; }
        }

        public class ReviewDto : GenericDto<int>
        {
            public int UserId { get; set; }
            public int ProductId { get; set; }
            public int Rating { get; set; }
            public string Comment { get; set; }
        }

        public class UserDto : GenericDto<int>
        {
            public string? Username { get; set; }
            public string? Email { get; set; }
            public string? PasswordHash { get; set; }
            public string? Lastname { get; set; }
            public string? Firstname { get; set; }
            public DateTime? Lastlogindate { get; set; }
            public string? Status { get; set; }
            public string? Role { get; set; }
            public string? Token { get; set; }
            public string? AvatarUrl { get; set; }
        }

        public class AddressDto : GenericDto<int>
        {
            public int UserId { get; set; }
            public string StreetAddress { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string PostalCode { get; set; }
            public string Country { get; set; }
        }

        public class OrderItemDto : GenericDto<int>
        {
            public int OrderId { get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
        }

        public class PaymentDto : GenericDto<int>
        {
            public int OrderId { get; set; }
            public string PaymentMethod { get; set; }
            public string PaymentStatus { get; set; }
            public decimal Amount { get; set; }
        }

        public class OrderDto : GenericDto<int>
        {
            public int UserId { get; set; }
            public string? OrderStatus { get; set; }
            public decimal TotalAmount { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? Address { get; set; }
            public string? Apartment { get; set; }
            public string? City { get; set; }
            public string? Country { get; set; }
            public string? Zipcode { get; set; }
            public string? ShippingMethod { get; set; }
            public OrderItemDto[]? OrderItems { get; set; }
        }

        public class AuthenticateRequest
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        public class AuthenticateResponse
        {
            public string UserName { get; set; }
            public string Token { get; set; }
        }

        public class ProductSalesData
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public string CategoryName { get; set; }
            public decimal UnitPrice { get; set; }
            public int QuantitySold { get; set; }
            public decimal TotalSales => UnitPrice * QuantitySold;
            public decimal Discount { get; set; }
            public decimal NetSales => TotalSales - Discount;
            public decimal CostOfGoodsSold { get; set; }
            public decimal ProfitMargin => NetSales - CostOfGoodsSold;
        }
    }
}
