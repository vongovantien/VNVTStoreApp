using System.IO;
using MediatR;
using VNVTStore.Application.Common;

namespace VNVTStore.Application.Promotions.Commands;

public record ImportPromotionsCommand(Stream FileStream) : IRequest<Result<int>>;
