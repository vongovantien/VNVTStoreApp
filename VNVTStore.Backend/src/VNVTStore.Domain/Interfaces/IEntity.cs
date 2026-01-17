namespace VNVTStore.Domain.Interfaces;

public interface IEntity
{
    string Code { get; set; }
    string? ModifiedType { get; set; }
    bool IsActive { get; set; }
    DateTime? CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}
