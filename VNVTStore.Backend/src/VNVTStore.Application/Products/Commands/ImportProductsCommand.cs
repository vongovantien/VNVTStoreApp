using System.IO;
using MediatR;
using VNVTStore.Application.Common;

namespace VNVTStore.Application.Products.Commands;

public record ImportProductsCommand(Stream FileStream) : IRequest<Result<int>>;
