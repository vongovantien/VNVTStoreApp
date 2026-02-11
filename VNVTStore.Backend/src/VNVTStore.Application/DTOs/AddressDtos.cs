namespace VNVTStore.Application.DTOs;

public class AddressDto : IBaseDto
{
    public string Code { get; set; } = null!;
    public string UserCode { get; set; } = null!;
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Category { get; set; }
    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool? IsDefault { get; set; }
}

public class CreateAddressDto
{
    public string? UserCode { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Category { get; set; }
    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool IsDefault { get; set; }
}

public class UpdateAddressDto
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Category { get; set; }
    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool? IsDefault { get; set; }
}
