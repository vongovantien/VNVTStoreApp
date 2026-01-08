using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;

namespace VNVTStore.Application.Categories.Commands;

public record CreateCategoryCommand(CategoryDto Dto) : IRequest<Result<CategoryDto>>;
