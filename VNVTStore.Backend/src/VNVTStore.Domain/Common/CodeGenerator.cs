namespace VNVTStore.Domain.Common;

/// <summary>
/// Centralised factory for generating entity codes.
/// Uses 16 hex characters (64 bits of entropy) from a random GUID, which gives a
/// collision probability well below 1-in-a-billion for realistic record counts.
///
/// Previous code used Substring(0, 10) = 40 bits, which starts showing collisions
/// around ~1 million records. 16 chars stays safe past ~4 billion records.
/// </summary>
public static class CodeGenerator
{
    /// <summary>
    /// Generates a 16-character lowercase hex code, e.g. "3f8a1b2c4d5e6f7a".
    /// Safe to store in VARCHAR(16) columns.
    /// </summary>
    public static string New() =>
        Guid.NewGuid().ToString("N")[..16];

    /// <summary>
    /// Generates a 16-character uppercase hex code, e.g. "3F8A1B2C4D5E6F7A".
    /// Useful for user-facing codes like coupons, promotions.
    /// </summary>
    public static string NewUpper() =>
        Guid.NewGuid().ToString("N")[..16].ToUpperInvariant();
}
