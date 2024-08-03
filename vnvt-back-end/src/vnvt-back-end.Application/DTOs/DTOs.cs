using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vnvt_back_end.Application.Interfaces;

namespace vnvt_back_end.Application.DTOs
{
    public class DTOs
    {

        public class UserProfileDto
        {
            public string Username { get; set; }
            public string Email { get; set; }
            public string FullName { get; set; }
            // Add other profile properties as needed
        }

        public class GenericDto<TKey> : IBaseDto
        {
            public int Id { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime UpdatedDate { get; set; }
        }
        public class ProductImageDto : GenericDto<int>
        {
            public string ImageUrl { get; set; }
            public int ProductId { get; set; }
        }

        public class ProductDto : GenericDto<int>
        {
            public string CategoryName { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public int CategoryId { get; set; }
            public int StockQuantity { get; set; }
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
            public string Username { get; set; }
            public string Email { get; set; }
            public string PasswordHash { get; set; }
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
            public string OrderStatus { get; set; }
            public decimal TotalAmount { get; set; }
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

    }
}
