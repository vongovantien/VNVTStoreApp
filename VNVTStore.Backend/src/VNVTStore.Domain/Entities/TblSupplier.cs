using VNVTStore.Domain.Common;

namespace VNVTStore.Domain.Entities;

public partial class TblSupplier : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? TaxCode { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public string? Notes { get; set; }
}
