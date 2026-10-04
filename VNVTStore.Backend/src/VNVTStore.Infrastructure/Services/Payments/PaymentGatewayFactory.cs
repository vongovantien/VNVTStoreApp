using System.Collections.Generic;
using System.Linq;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Enums;

namespace VNVTStore.Infrastructure.Services.Payments;

public class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly IEnumerable<IPaymentGateway> _gateways;

    public PaymentGatewayFactory(IEnumerable<IPaymentGateway> gateways)
    {
        _gateways = gateways;
    }

    public IPaymentGateway? Get(PaymentMethod method)
    {
        return _gateways.FirstOrDefault(g => g.Method == method);
    }
}
