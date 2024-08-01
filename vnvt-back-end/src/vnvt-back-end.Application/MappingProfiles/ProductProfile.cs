using AutoMapper;
using vnvt_back_end.Application.DTOs;
using vnvt_back_end.Infrastructure;

namespace vnvt_back_end.Application.MappingProfiles
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductDto>().ReverseMap();
        }
    }
}
