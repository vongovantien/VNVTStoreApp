using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Common.Queries
{
    public record GetShopConfigsQuery() : IRequest<Result<List<ShopConfigDto>>>;
    public record GetConfigByCodeQuery(string Code) : IRequest<Result<ShopConfigDto>>;
}

namespace VNVTStore.Application.Common.Commands
{
    public record UpdateConfigCommand(string Code, UpdateConfigDto Dto) : IRequest<Result<ShopConfigDto>>;
}
