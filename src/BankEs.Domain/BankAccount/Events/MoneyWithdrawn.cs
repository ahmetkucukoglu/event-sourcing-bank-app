using BankEs.Domain.BankMoney;

namespace BankEs.Domain.BankAccount.Events;

public record MoneyWithdrawn(Money Money);