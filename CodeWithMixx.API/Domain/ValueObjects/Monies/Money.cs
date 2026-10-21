using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Domain.ValueObjects.Monies;

public record Money
{
    public decimal Value { get; private init; }
    
    private Money() { }
    private Money(decimal value) => Value = Math.Round(value, 2);
    
    public static Money Zero => new(0);

    public static Result<Money> Create(decimal value)
    {
        if (value < 0)
            return Result<Money>.Failure(MoneyError.NegativeValue());
        
        return Result<Money>.Success(new Money(value));
    }

    public static Result<Money> Add(decimal firstValue, decimal secondValue)
    {
        if(firstValue < 0 || secondValue < 0)
            return Result<Money>.Failure(MoneyError.NegativeValue());

        return Result<Money>.Success(new Money(firstValue + secondValue));
    }

    public static Result<Money> Subtract(Money firstValue, decimal secondValue)
    {
        if(secondValue < 0)
            return Result<Money>.Failure(MoneyError.NegativeValue());
        
        if(firstValue.Value < secondValue)
            return Result<Money>.Failure(MoneyError.OldValueIsHigher());

        return Result<Money>.Success(new Money(firstValue.Value - secondValue));
    }
    
    public Result<Money> Subtract(Money money)
    {
        if(money.Value < 0)
            return Result<Money>.Failure(MoneyError.NegativeValue());
        
        if(Value < money.Value)
            return Result<Money>.Failure(MoneyError.OldValueIsHigher());

        return Result<Money>.Success(new Money(Value - money.Value));
    }
    
    public static Money operator +(Money firstValue, Money secondValue) => new(firstValue.Value + secondValue.Value);
    public static Result<Money> operator -(Money firstValue, Money secondValue) => Subtract(firstValue, secondValue.Value);
    public static bool operator >(Money firstValue, Money secondValue) => firstValue.Value > secondValue.Value;
    public static bool operator <(Money firstValue, Money secondValue) => firstValue.Value < secondValue.Value;
    public static bool operator >=(Money firstValue, Money secondValue) => firstValue.Value >= secondValue.Value;
    public static bool operator <=(Money firstValue, Money secondValue) => firstValue.Value <= secondValue.Value;
};