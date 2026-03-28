using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace VNVTStore.Application.SystemConfig.Commands;

public record ImportSystemConfigsCommand(Stream FileStream) : IRequest<Result<int>>;

public class ImportSystemConfigsCommandHandler : IRequestHandler<ImportSystemConfigsCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;

    public ImportSystemConfigsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(ImportSystemConfigsCommand request, CancellationToken cancellationToken)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage(request.FileStream);
        var worksheet = package.Workbook.Worksheets[0];
        var rowCount = worksheet.Dimension.Rows;

        if (rowCount < 2) return Result<int>.Failure(Error.Validation("SystemConfig.Import.Empty", "File is empty"));

        var count = 0;
        for (int row = 2; row <= rowCount; row++)
        {
            var code = worksheet.Cells[row, 1].Value?.ToString();
            var value = worksheet.Cells[row, 2].Value?.ToString();
            var description = worksheet.Cells[row, 3].Value?.ToString();
            var isActiveStr = worksheet.Cells[row, 4].Value?.ToString();

            if (string.IsNullOrEmpty(code)) continue;

            var config = await _context.TblSystemConfigs
                .FirstOrDefaultAsync(c => c.Code == code, cancellationToken);

            if (config == null)
            {
                config = new TblSystemConfig
                {
                    Code = code,
                    ConfigValue = value ?? "",
                    Description = description,
                    IsActive = string.IsNullOrEmpty(isActiveStr) || bool.Parse(isActiveStr),
                    CreatedAt = DateTime.UtcNow
                };
                _context.TblSystemConfigs.Add(config);
            }
            else
            {
                config.ConfigValue = value ?? "";
                config.Description = description;
                config.IsActive = string.IsNullOrEmpty(isActiveStr) || bool.Parse(isActiveStr);
                config.UpdatedAt = DateTime.UtcNow;
            }
            count++;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(count);
    }
}
