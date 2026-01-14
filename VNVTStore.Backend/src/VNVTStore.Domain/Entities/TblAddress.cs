using System;
using System.Collections.Generic;

namespace VNVTStore.Domain.Entities;

public record AddressDetails(
    string AddressLine,
    string? City = null,
    string? State = null,
    string? PostalCode = null,
    string? Country = "Vietnam"
);

public partial class TblAddress
{
    private TblAddress() { }

    public string Code { get; private set; } = null!;

    public string UserCode { get; private set; } = null!;

    public string AddressLine { get; private set; } = null!;

    public string? City { get; private set; }

    public string? State { get; private set; }

    public string? PostalCode { get; private set; }

    public string? Country { get; private set; }

    public bool? IsDefault { get; private set; }

    public DateTime? CreatedAt { get; private set; }

    public virtual ICollection<TblOrder> TblOrders { get; private set; } = new List<TblOrder>();

    public virtual TblUser UserCodeNavigation { get; private set; } = null!;

    public void SetAsDefault() => IsDefault = true;
    public void UnsetDefault() => IsDefault = false;

    public void Update(AddressDetails details, bool? isDefault = null)
    {
        AddressLine = details.AddressLine;
        City = details.City;
        State = details.State;
        PostalCode = details.PostalCode;
        Country = details.Country;
        if (isDefault.HasValue) IsDefault = isDefault;
    }

    public class Builder
    {
        private string _userCode = null!;
        private AddressDetails _details = null!;
        private bool _isDefault;

        public Builder WithUser(string userCode)
        {
            _userCode = userCode;
            return this;
        }

        public Builder AtLocation(string addressLine, string? city = null, string? state = null, string? postalCode = null, string? country = "Vietnam")
        {
            _details = new AddressDetails(addressLine, city, state, postalCode, country);
            return this;
        }

        public Builder SetAsDefault()
        {
            _isDefault = true;
            return this;
        }

        public TblAddress Build()
        {
            if (string.IsNullOrEmpty(_userCode)) throw new InvalidOperationException("UserCode is required");
            if (_details == null) throw new InvalidOperationException("Address details are required");

            return new TblAddress
            {
                Code = Guid.NewGuid().ToString("N").Substring(0, 10),
                UserCode = _userCode,
                AddressLine = _details.AddressLine,
                City = _details.City,
                State = _details.State,
                PostalCode = _details.PostalCode,
                Country = _details.Country,
                IsDefault = _isDefault,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
