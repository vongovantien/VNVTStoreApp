using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.Common.Helpers;
using VNVTStore.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace VNVTStore.Application.SystemConfig.Queries;

public record ExportSystemConfigsQuery : IRequest<Result<byte[]>>;

public class ExportSystemConfigsQueryHandler : IRequestHandler<ExportSystemConfigsQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _context;

    public ExportSystemConfigsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<byte[]>> Handle(ExportSystemConfigsQuery request, CancellationToken cancellationToken)
    {
        var configs = await _context.TblSystemConfigs
            .AsNoTracking()
            .Select(c => new 
            {
                Code = c.Code,
                Value = c.ConfigValue,
                Description = c.Description,
                IsActive = c.IsActive
            })
            .ToListAsync(cancellationToken);

        var fileBytes = ExcelExportHelper.ExportToExcel(configs, "SystemConfigs");
        return Result<byte[]>.Success(fileBytes);
    }
}
