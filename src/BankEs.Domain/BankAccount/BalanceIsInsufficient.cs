namespace BankEs.Domain.BankAccount;

public class BalanceIsInsufficient : Exception
{
    public BalanceIsInsufficient() : base("The balance is insufficient.")
    {
    }
}