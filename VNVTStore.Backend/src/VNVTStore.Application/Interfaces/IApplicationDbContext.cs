using Microsoft.EntityFrameworkCore;
using VNVTStore.Domain.Entities;

namespace VNVTStore.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TblUser> TblUsers { get; }
    DbSet<TblProduct> TblProducts { get; }
    DbSet<TblQuote> TblQuotes { get; }
    DbSet<TblBanner> TblBanners { get; }
    DbSet<TblFile> TblFiles { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
