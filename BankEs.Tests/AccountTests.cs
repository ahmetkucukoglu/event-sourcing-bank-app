using BankEs.Domain.BankAccount;
using BankEs.Domain.BankAccount.Events;
using BankEs.Domain.BankCustomer;
using BankEs.Domain.BankMoney;

namespace BankEs.Tests;

public class AccountTests
{
    [Fact]
    public void Should_Be_True_When_AddEvents()
    {
        var bankAccount = new Account(new AccountId(Guid.NewGuid()));
        bankAccount.CreateAccount(new CustomerId(Guid.NewGuid()), Currency.TL);
        bankAccount.DepositMoney(new Money(500M, Currency.TL));
        bankAccount.WithdrawMoney(new Money(250M, Currency.TL));

        Assert.Equal(new Money(250M, Currency.TL), bankAccount.State.Balance);
        Assert.Equal(3, bankAccount.GetChanges().Length);
        Assert.Collection(bankAccount.GetChanges(),
            c => Assert.IsType<AccountCreated>(c),
            c => Assert.IsType<MoneyDeposited>(c),
            c => Assert.IsType<MoneyWithdrawn>(c));
    }

    [Fact]
    public void Should_Be_True_When_LoadChanges()
    {
        var bankAccount = new Account(new AccountId(Guid.NewGuid()));
        bankAccount.LoadChange(0, new AccountCreated(new CustomerId(Guid.NewGuid()), Currency.TL));
        bankAccount.LoadChange(1, new MoneyDeposited(new Money(500M, Currency.TL)));
        bankAccount.LoadChange(2, new MoneyWithdrawn(new Money(250M, Currency.TL)));

        Assert.Equal(new Money(250M, Currency.TL), bankAccount.State.Balance);
        Assert.Empty(bankAccount.GetChanges());
        Assert.Equal(2, bankAccount.Version);
    }

    [Fact]
    public void Should_Be_Success_When_LoadState()
    {
        var bankAccount = new Account(
            new AccountId(Guid.NewGuid()),
            new AccountSnapshot(
                new AccountState
                {
                    CustomerId = new CustomerId(Guid.NewGuid()),
                    Balance = new Money(250M, Currency.TL)
                },
                1));
        bankAccount.WithdrawMoney(new Money(250M, Currency.TL));

        Assert.Equal(Money.Zero(Currency.TL), bankAccount.State.Balance);
        Assert.Equal(1, bankAccount.Version);
        Assert.Single(bankAccount.GetChanges());
        Assert.Collection(bankAccount.GetChanges(),
            c => Assert.IsType<MoneyWithdrawn>(c));
    }
}