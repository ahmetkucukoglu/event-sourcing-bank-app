using BankEs.Domain.BankCustomer;
using BankEs.Domain.BankMoney;

namespace BankEs.Domain.BankAccount.Events;

public record AccountCreated(CustomerId CustomerId, Currency Currency);