using BankEs.Domain.BankAccount.Events;
using BankEs.Domain.BankCustomer;
using BankEs.Domain.BankMoney;

namespace BankEs.Domain.BankAccount;

public class Account
{
    public AccountId Id { get; }
    public AccountState State { get; } = new();

    public long Version { get; private set; } = -1;

    readonly IList<object> _changes = new List<object>();
    public object[] GetChanges() => _changes.ToArray();

    public Account(AccountId id)
    {
        Id = id;
    }

    public Account(AccountId id, AccountSnapshot snapshot)
    {
        Id = id;
        State = snapshot.State;
        Version = snapshot.Version;
    }

    public void CreateAccount(CustomerId customerId, Currency currency)
    {
        AddEvent(new AccountCreated(customerId, currency));
    }

    private void Apply(AccountCreated @event)
    {
        State.CustomerId = @event.CustomerId;
        State.Balance = Money.Zero(@event.Currency);
    }

    public void DepositMoney(Money money)
    {
        AddEvent(new MoneyDeposited(money));
    }

    private void Apply(MoneyDeposited @event)
    {
        State.Balance += @event.Money;
    }

    public void WithdrawMoney(Money money)
    {
        if (State.Balance - money < Money.Zero(State.Balance.Currency)) throw new BalanceIsInsufficient();

        AddEvent(new MoneyWithdrawn(money));
    }

    private void Apply(MoneyWithdrawn @event)
    {
        State.Balance -= @event.Money;
    }

    private void AddEvent(object @event)
    {
        ApplyEvent(@event);

        _changes.Add(@event);
    }

    private void ApplyEvent(object @event)
    {
        switch (@event)
        {
            case AccountCreated createAccount:
                Apply(createAccount);
                break;
            case MoneyDeposited depositMoney:
                Apply(depositMoney);
                break;
            case MoneyWithdrawn withdrawMoney:
                Apply(withdrawMoney);
                break;
        }
    }

    public void LoadChange(long version, object change)
    {
        Version = version;

        ApplyEvent(change);
    }

    public void ClearChanges() => _changes.Clear();
}