using VNVTStore.Domain.Interfaces;

namespace VNVTStore.Domain.Entities;

public class TblUserLogin
{
    public string LoginProvider { get; set; } = null!; // e.g. "Google", "Facebook"
    public string ProviderKey { get; set; } = null!; // The unique ID from the provider
    public string? ProviderDisplayName { get; set; } // e.g. User's email or name on that platform
    
    public string UserId { get; set; } = null!;
    public virtual TblUser User { get; set; } = null!;
}
