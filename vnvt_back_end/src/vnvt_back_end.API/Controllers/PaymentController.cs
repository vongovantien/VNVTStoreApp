using Microsoft.AspNetCore.Mvc;
using vnvt_back_end.Application.Interfaces;
using vnvt_back_end.Infrastructure;
using static vnvt_back_end.Application.DTOs.DTOs;

namespace vnvt_back_end.API.Controllers
{
    [Route("api/[controller]")]
    //[MiddlewareFilter(typeof(RequestLocalizationMiddleware))]
    [ApiController]
    public class PaymentsController : BaseController<PaymentDto, Payment>
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
           : base(paymentService)
        {
            _paymentService = paymentService;
        }
    }
}
