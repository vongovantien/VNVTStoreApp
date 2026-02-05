using MediatR;

namespace VNVTStore.Application.Dashboard.Queries;

public record GetSystemCountsQuery() : IRequest<GetSystemCountsResult>;

public class GetSystemCountsResult
{
    public object? AdminUser { get; set; }
    public int UsersCount { get; set; }
    public IEnumerable<object> SampleUserRoles { get; set; } = [];
    public int Products { get; set; }
    public int Categories { get; set; }
    public int Orders { get; set; }
    public decimal TotalRevenue { get; set; }
    public DateTime? LatestOrderDate { get; set; }
    public int ThisMonthOrdersCount { get; set; }
    public DateTime UtcNow { get; set; }
    public DateTime ThisMonthStart { get; set; }
    public int Banners { get; set; }
    public int Suppliers { get; set; }
}
