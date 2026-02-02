using System;
using System.Collections.Generic;

namespace VNVTStore.Domain.ValueObjects;

public class BinLocation : IEquatable<BinLocation>
{
    public string Value { get; private set; }

    private BinLocation(string value)
    {
        Value = value;
    }

    public static BinLocation Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Bin location cannot be empty", nameof(value));

        if (value.Length > 20)
            throw new ArgumentException("Bin location cannot exceed 20 characters", nameof(value));

        return new BinLocation(value);
    }

    public static implicit operator string(BinLocation binLocation) => binLocation.Value;
    public static implicit operator BinLocation(string value) => Create(value);

    public bool Equals(BinLocation? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is BinLocation other && Equals(other);
    }

    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;
}
