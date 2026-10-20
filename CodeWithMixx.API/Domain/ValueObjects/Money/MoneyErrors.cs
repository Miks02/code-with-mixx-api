using CodeWithMixx.API.Common.Result;

namespace CodeWithMixx.API.Domain.ValueObjects.Money;

public static class MoneyError
{
    public static Error NegativeValue()
        => new Error("NegativeValue", "Money value cannot be negative.");
    public static Error OldValueIsHigher()
        => new Error("OldValueIsHigher", "Old value is higher than new value.");
}