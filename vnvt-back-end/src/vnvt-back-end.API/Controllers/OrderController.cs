using Microsoft.AspNetCore.Mvc;
using vnvt_back_end.Application.DTOs;
using vnvt_back_end.Application.Interfaces;
using vnvt_back_end.Infrastructure;
using static vnvt_back_end.Application.DTOs.DTOs;

namespace vnvt_back_end.API.Controllers
{
    [Route("api/[controller]")]
    //[MiddlewareFilter(typeof(RequestLocalizationMiddleware))]
    [ApiController]
    public class OrdersController : BaseController<OrderDto, Order>
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
           : base(orderService)
        {
            _orderService = orderService;
        }
    }
}
