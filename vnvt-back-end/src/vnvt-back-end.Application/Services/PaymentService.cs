using AutoMapper;
using System.Linq.Expressions;
using vnvt_back_end.Application.DTOs;
using vnvt_back_end.Application.Interfaces;
using vnvt_back_end.Application.Models;
using vnvt_back_end.Application.Utils;
using vnvt_back_end.Infrastructure;
using static vnvt_back_end.Application.DTOs.DTOs;

namespace vnvt_back_end.Application.Services
{
    public class PaymentService : BaseService<Payment, PaymentDto>, IPaymentService
    {
        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper)
        {
        }
    }
}