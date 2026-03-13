namespace VNVTStore.Application.Common.Interfaces;

/// <summary>
/// Marker interface for commands that should be automatically audited.
/// </summary>
public interface IAuditableCommand
{
    // The name of the action to log (e.g., "CREATE_PRODUCT")
    // If null, the behavior can derive it from the command type name.
    string? AuditAction => null;
    
    // The identifier of the resource being acted upon (e.g., Product Code)
    // If null, the behavior can try to find a 'Code' property on the command.
    string? AuditResourceId => null;
}
