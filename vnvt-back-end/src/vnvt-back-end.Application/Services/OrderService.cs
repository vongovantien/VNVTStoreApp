using AutoMapper;
using vnvt_back_end.Application.DTOs;
using vnvt_back_end.Application.Interfaces;
using vnvt_back_end.Application.Models;
using vnvt_back_end.Application.Utils;
using vnvt_back_end.Infrastructure;
using static vnvt_back_end.Application.DTOs.DTOs;

namespace vnvt_back_end.Application.Services
{
    public class OrderService : BaseService<Order, OrderDto>, IOrderService
    {
        public OrderService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
        {
        }

        public virtual async Task<ApiResponse<OrderDto>> AddAsync(OrderDto dto)
        {
            decimal totalAmount = 0;
            foreach (var item in dto.OrderItems)
            {
                totalAmount += item.Quantity * item.Price;
            }
            var result = await base.AddAsync(dto);
            return result;
        }
    }
}
