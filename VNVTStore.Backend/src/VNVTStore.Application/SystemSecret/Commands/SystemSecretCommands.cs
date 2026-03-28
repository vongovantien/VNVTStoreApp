using MediatR;
using System.Threading;
using System.Threading.Tasks;
using VNVTStore.Application.Common;
using VNVTStore.Application.Interfaces;

namespace VNVTStore.Application.SystemSecret.Commands;

public record UpdateSystemSecretCommand(string Key, string Value, string? Description) : IRequest<Result<bool>>;

public class UpdateSystemSecretCommandHandler : IRequestHandler<UpdateSystemSecretCommand, Result<bool>>
{
    private readonly ISecretConfigurationService _secretConfig;

    public UpdateSystemSecretCommandHandler(ISecretConfigurationService secretConfig)
    {
        _secretConfig = secretConfig;
    }

    public async Task<Result<bool>> Handle(UpdateSystemSecretCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Key))
            return Result.Failure<bool>(Error.Validation("Key cannot be empty"));

        await _secretConfig.SetSecretAsync(request.Key, request.Value, request.Description);

        return Result.Success(true);
    }
}

public record DeleteSystemSecretCommand(string Key) : IRequest<Result<bool>>;

public class DeleteSystemSecretCommandHandler : IRequestHandler<DeleteSystemSecretCommand, Result<bool>>
{
    private readonly ISecretConfigurationService _secretConfig;

    public DeleteSystemSecretCommandHandler(ISecretConfigurationService secretConfig)
    {
        _secretConfig = secretConfig;
    }

    public async Task<Result<bool>> Handle(DeleteSystemSecretCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Key))
            return Result.Failure<bool>(Error.Validation("Key cannot be empty"));

        var exists = await _secretConfig.HasSecretAsync(request.Key);
        if (!exists)
            return Result.Failure<bool>(Error.NotFound("Secret", request.Key));

        await _secretConfig.DeleteSecretAsync(request.Key);
        return Result.Success(true);
    }
}
