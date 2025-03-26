using vnvt_back_end.Infrastructure;
using static vnvt_back_end.Application.DTOs.DTOs;

namespace vnvt_back_end.Application.Interfaces
{
    public interface IPaymentService : IBaseService<PaymentDto, Payment>
    {
    }
}
