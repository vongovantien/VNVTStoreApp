namespace VNVTStore.Domain.Common;

using VNVTStore.Domain.Interfaces;

/// <summary>
/// Abstract base class that implements all common IEntity properties.
/// Every domain entity should inherit from this instead of implementing IEntity directly.
/// This eliminates the need to repeat IsActive, IsFixed, CreatedAt, UpdatedAt,
/// CreatedBy, UpdatedBy, ModifiedType in every entity class.
/// </summary>
public abstract class BaseEntity : IEntity
{
    public string Code { get; set; } = CodeGenerator.New();

    public bool IsActive { get; set; } = true;

    /// <inheritdoc />
    public bool IsFixed { get; set; } = false;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    /// <inheritdoc />
    public string? CreatedBy { get; set; }

    /// <inheritdoc />
    public string? UpdatedBy { get; set; }

    public string? ModifiedType { get; set; }
}
