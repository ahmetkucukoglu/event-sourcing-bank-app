namespace BankEs.Domain.BankMoney;

public record Money
{
    public Money(decimal amount, Currency currency)
    {
        ArgumentNullException.ThrowIfNull(amount);
        ArgumentNullException.ThrowIfNull(currency);

        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }
    public Currency Currency { get; }

    public override string ToString() => $"{Amount}{Currency.Symbol}";

    public static Money Zero(Currency currency)
    {
        return new Money(0, currency);
    }

    public static bool operator <(Money obj1, Money obj2)
    {
        ThrowIfCurrencyIsNotMatch(obj1, obj2);

        return obj1.Amount < obj2.Amount;
    }

    public static bool operator >(Money obj1, Money obj2)
    {
        ThrowIfCurrencyIsNotMatch(obj1, obj2);

        return obj1.Amount > obj2.Amount;
    }

    public static Money operator +(Money obj1, Money obj2)
    {
        ThrowIfCurrencyIsNotMatch(obj1, obj2);

        return new Money(obj1.Amount + obj2.Amount, obj1.Currency);
    }

    public static Money operator -(Money obj1, Money obj2)
    {
        ThrowIfCurrencyIsNotMatch(obj1, obj2);

        return new Money(obj1.Amount - obj2.Amount, obj1.Currency);
    }

    private static void ThrowIfCurrencyIsNotMatch(Money obj1, Money obj2)
    {
        if (obj1.Currency != obj2.Currency) throw new CurrencyIsNotValid();
    }
}