namespace VNVTStore.Application.Interfaces;

/// <summary>
/// Password hasher interface
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hashedPassword);
}
