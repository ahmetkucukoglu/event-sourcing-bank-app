using BankEs.Domain.BankMoney;

namespace BankEs.Domain.BankAccount.Events;

public record MoneyDeposited(Money Money);