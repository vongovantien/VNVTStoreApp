using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.Common.Helpers;
using VNVTStore.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace VNVTStore.Application.SystemSecret.Queries;

public record ExportSystemSecretsQuery : IRequest<Result<byte[]>>;

public class ExportSystemSecretsQueryHandler : IRequestHandler<ExportSystemSecretsQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _context;

    public ExportSystemSecretsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<byte[]>> Handle(ExportSystemSecretsQuery request, CancellationToken cancellationToken)
    {
        var secrets = await _context.TblSystemSecrets
            .AsNoTracking()
            .Select(s => new 
            {
                Code = s.Code,
                Value = s.SecretValue,
                Description = s.Description,
                IsActive = s.IsActive,
                IsEncrypted = s.IsEncrypted
            })
            .ToListAsync(cancellationToken);

        var fileBytes = ExcelExportHelper.ExportToExcel(secrets, "SystemSecrets");
        return Result<byte[]>.Success(fileBytes);
    }
}
