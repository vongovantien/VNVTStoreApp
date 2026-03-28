using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VNVTStore.Application.Common;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Interfaces;

namespace VNVTStore.Application.SystemSecret.Queries;

public record GetSystemSecretsQuery : IRequest<Result<IEnumerable<SystemSecretDto>>>;

public class GetSystemSecretsQueryHandler : IRequestHandler<GetSystemSecretsQuery, Result<IEnumerable<SystemSecretDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetSystemSecretsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<SystemSecretDto>>> Handle(GetSystemSecretsQuery request, CancellationToken cancellationToken)
    {
        var secrets = await _context.TblSystemSecrets
            .Select(s => new SystemSecretDto
            {
                Code = s.Code,
                SecretValue = s.SecretValue,
                Description = s.Description,
                IsEncrypted = s.IsEncrypted,
                IsActive = s.IsActive,
                UpdatedAt = s.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return Result.Success(secrets.AsEnumerable());
    }
}
