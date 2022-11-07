using BankEs.Domain.BankCustomer;
using BankEs.Domain.BankMoney;

namespace BankEs.Domain.BankAccount;

public class AccountState
{ 
    public CustomerId CustomerId { get; set; }
    public Money Balance { get; set; }
}