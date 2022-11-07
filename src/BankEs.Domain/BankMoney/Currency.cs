using System.Text.RegularExpressions;

namespace BankEs.Domain.BankMoney;

public record Currency
{
    public static Currency TL = GetByName("TL");
    public static Currency USD = GetByName("USD");

    public Currency(string name, string symbol)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(symbol);

        Name = name;
        Symbol = symbol;
    }

    public string Name { get; }
    public string Symbol { get; }

    public static Currency GetByName(string name)
    {
        return name switch
        {
            "TL" => new Currency("TL", "₺"),
            "USD" => new Currency("USD", "$")
        };
    }

    public override string ToString() => Name;

    public static implicit operator string(Currency currency) => currency.ToString();
}