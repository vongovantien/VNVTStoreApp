using MediatR;
using VNVTStore.Application.Common;
using VNVTStore.Application.Interfaces;
using VNVTStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace VNVTStore.Application.SystemSecret.Commands;

public record ImportSystemSecretsCommand(Stream FileStream) : IRequest<Result<int>>;

public class ImportSystemSecretsCommandHandler : IRequestHandler<ImportSystemSecretsCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;

    public ImportSystemSecretsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(ImportSystemSecretsCommand request, CancellationToken cancellationToken)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage(request.FileStream);
        var worksheet = package.Workbook.Worksheets[0];
        var rowCount = worksheet.Dimension.Rows;

        if (rowCount < 2) return Result<int>.Failure(Error.Validation("SystemSecret.Import.Empty", "File is empty"));

        var count = 0;
        for (int row = 2; row <= rowCount; row++)
        {
            var code = worksheet.Cells[row, 1].Value?.ToString();
            var value = worksheet.Cells[row, 2].Value?.ToString();
            var description = worksheet.Cells[row, 3].Value?.ToString();
            var isActiveStr = worksheet.Cells[row, 4].Value?.ToString();
            var isEncryptedStr = worksheet.Cells[row, 5].Value?.ToString();

            if (string.IsNullOrEmpty(code)) continue;

            var secret = await _context.TblSystemSecrets
                .FirstOrDefaultAsync(s => s.Code == code, cancellationToken);

            if (secret == null)
            {
                secret = new TblSystemSecret
                {
                    Code = code,
                    SecretValue = value ?? "",
                    Description = description,
                    IsActive = string.IsNullOrEmpty(isActiveStr) || bool.Parse(isActiveStr),
                    IsEncrypted = !string.IsNullOrEmpty(isEncryptedStr) && bool.Parse(isEncryptedStr),
                    CreatedAt = DateTime.UtcNow
                };
                _context.TblSystemSecrets.Add(secret);
            }
            else
            {
                secret.SecretValue = value ?? "";
                secret.Description = description;
                secret.IsActive = string.IsNullOrEmpty(isActiveStr) || bool.Parse(isActiveStr);
                secret.IsEncrypted = !string.IsNullOrEmpty(isEncryptedStr) && bool.Parse(isEncryptedStr);
                secret.UpdatedAt = DateTime.UtcNow;
            }
            count++;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(count);
    }
}
