using BankEs.Domain.BankAccount;
using BankEs.Domain.BankCustomer;
using BankEs.Domain.BankMoney;
using BankEs.Infrastructure;
using EventStore.Client;

namespace BankEs.Tests;

public class EventStoreTests
{
    private readonly AccountAggregateRepository _accountAggregateRepository;

    public EventStoreTests()
    {
        var eventStoreClient = new EventStoreClient(EventStoreClientSettings.Create("esdb://localhost:2113?tls=false"));

        _accountAggregateRepository = new AccountAggregateRepository(eventStoreClient);
    }

    [Fact]
    public async Task Should_Be_True_When_AddEvents()
    {
        var accountId = new AccountId(Guid.NewGuid());

        var account = await _accountAggregateRepository.GetAsync(accountId);
        account.CreateAccount(new CustomerId(Guid.NewGuid()), Currency.TL);
        account.DepositMoney(new Money(100M, Currency.TL));
        account.WithdrawMoney(new Money(50M, Currency.TL));
        await _accountAggregateRepository.SaveAsync(account);

        var account1 = await _accountAggregateRepository.GetAsync(accountId);

        Assert.Equal("50₺", account1.State.Balance.ToString());
    }
    
    [Fact]
    public async Task Should_Be_True_When_Concurrency()
    {
        var accountId = new AccountId(Guid.NewGuid());

        var account = await _accountAggregateRepository.GetAsync(accountId);
        account.CreateAccount(new CustomerId(Guid.NewGuid()), Currency.TL);
        account.DepositMoney(new Money(100M, Currency.TL));
        await _accountAggregateRepository.SaveAsync(account);
        
        var account1 = await _accountAggregateRepository.GetAsync(accountId);
        account1.WithdrawMoney(new Money(100M, Currency.TL));
        
        var account2 = await _accountAggregateRepository.GetAsync(accountId);
        account2.WithdrawMoney(new Money(100M, Currency.TL));

        await _accountAggregateRepository.SaveAsync(account1);
        await Assert.ThrowsAsync<WrongExpectedVersionException>(() => _accountAggregateRepository.SaveAsync(account2));
        
        var account3 = await _accountAggregateRepository.GetAsync(accountId);
        
        Assert.Equal("0₺", account3.State.Balance.ToString());
    }
    
    [Fact]
    public async Task Should_Be_True_When_Snapshot()
    {
        var accountId = new AccountId(Guid.NewGuid());

        var account = await _accountAggregateRepository.GetAsync(accountId);
        account.CreateAccount(new CustomerId(Guid.NewGuid()), Currency.TL);
        account.DepositMoney(new Money(100M, Currency.TL));
        account.WithdrawMoney(new Money(50M, Currency.TL));
        account.DepositMoney(new Money(100M, Currency.TL));
        account.WithdrawMoney(new Money(50M, Currency.TL));
        account.DepositMoney(new Money(100M, Currency.TL));
        await _accountAggregateRepository.SaveAsync(account);
        
        account = await _accountAggregateRepository.GetAsync(accountId);
        account.WithdrawMoney(new Money(50M, Currency.TL));
        account.DepositMoney(new Money(100M, Currency.TL));
        account.WithdrawMoney(new Money(50M, Currency.TL));
        account.DepositMoney(new Money(100M, Currency.TL));
        account.WithdrawMoney(new Money(50M, Currency.TL));
        await _accountAggregateRepository.SaveAsync(account);
        
        account = await _accountAggregateRepository.GetAsync(accountId);
        account.DepositMoney(new Money(100M, Currency.TL));
        account.DepositMoney(new Money(100M, Currency.TL));
        await _accountAggregateRepository.SaveAsync(account);

        var account1 = await _accountAggregateRepository.GetAsync(accountId);

        Assert.Equal("450₺", account1.State.Balance.ToString());
    }
}