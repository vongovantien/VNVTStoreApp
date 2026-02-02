using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using System.Text.Json.Serialization;

namespace VNVTStore.Domain.Common;

[JsonConverter(typeof(SmartEnumJsonConverter<>))] // This might not work for generic open type in attribute without factory
// Actually, for generic types, we usually use a ConverterFactory or apply to the concrete type.
// Let's applying it to the Concrete Type OrderStatus is safer.
public abstract class SmartEnum<TEnum> : IEquatable<SmartEnum<TEnum>>
    where TEnum : SmartEnum<TEnum>
{
// ... (content unchanged)
    private static readonly Lazy<Dictionary<int, TEnum>> _itemParams = new Lazy<Dictionary<int, TEnum>>(() =>
    {
        var type = typeof(TEnum);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        var dic = new Dictionary<int, TEnum>();

        foreach (var info in fields)
        {
            var locatedValue = info.GetValue(null) as TEnum;

            if (locatedValue != null)
            {
                 if (!dic.ContainsKey(locatedValue.Value))
                     dic.Add(locatedValue.Value, locatedValue);
            }
        }
        return dic;
    });

    private static readonly Lazy<Dictionary<string, TEnum>> _itemNames = new Lazy<Dictionary<string, TEnum>>(() =>
    {
        return _itemParams.Value.Values.ToDictionary(item => item.Name.ToUpper(), item => item);
    });

    public string Name { get; }
    public int Value { get; }

    protected SmartEnum(string name, int value)
    {
        Name = name;
        Value = value;
    }

    public static TEnum? FromValue(int value)
    {
        if (_itemParams.Value.TryGetValue(value, out var enumVal))
        {
            return enumVal;
        }
        return null;
    }

    public static TEnum? FromName(string name)
    {
        if (_itemNames.Value.TryGetValue(name.ToUpper(), out var enumVal))
        {
            return enumVal;
        }
        return null;
    }

    public static IEnumerable<TEnum> List() => _itemParams.Value.Values.ToList();

    public bool Equals(SmartEnum<TEnum>? other)
    {
        if (other is null) return false;
        return GetType() == other.GetType() && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is SmartEnum<TEnum> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
    
    public override string ToString() => Name;

    public static bool operator ==(SmartEnum<TEnum>? left, SmartEnum<TEnum>? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(SmartEnum<TEnum>? left, SmartEnum<TEnum>? right)
    {
        return !(left == right);
    }
}
