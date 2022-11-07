namespace BankEs.Domain.BankMoney;

public class CurrencyIsNotValid : Exception
{
    public CurrencyIsNotValid() : base("The currency isn't valid.")
    {
    }
}