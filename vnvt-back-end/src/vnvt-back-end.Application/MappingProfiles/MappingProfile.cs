using AutoMapper;
using vnvt_back_end.Infrastructure;
using static vnvt_back_end.Application.DTOs.DTOs;

namespace vnvt_back_end.Application.MappingProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductDto>().ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name)).ReverseMap();
            CreateMap<Order, OrderDto>()
             .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems)).ReverseMap();
            CreateMap<OrderItem, OrderItemDto>().ReverseMap();
            CreateMap<Review, ReviewDto>().ReverseMap();
            CreateMap<Payment, PaymentDto>().ReverseMap();
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<ProductImage, ProductImageDto>();
        }
    }
}
