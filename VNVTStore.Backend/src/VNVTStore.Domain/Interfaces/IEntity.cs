namespace VNVTStore.Domain.Interfaces;

/// <summary>
/// Marker interface for all domain entities.
/// Concrete implementations should inherit <see cref="VNVTStore.Domain.Common.BaseEntity"/>
/// rather than implementing this interface directly.
/// </summary>
public interface IEntity
{
    string Code { get; set; }
    bool IsActive { get; set; }

    /// <summary>
    /// System-managed flag. When true the record cannot be deleted and
    /// Insert/Update operations must NOT change this value.
    /// </summary>
    bool IsFixed { get => false; set { } }

    DateTime? CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }

    /// <summary>UserCode of the creator.</summary>
    string? CreatedBy { get => null; set { } }

    /// <summary>UserCode of the last editor.</summary>
    string? UpdatedBy { get => null; set { } }

    string? ModifiedType { get => null; set { } }
}
