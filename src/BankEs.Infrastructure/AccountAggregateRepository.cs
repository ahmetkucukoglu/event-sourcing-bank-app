using System.Text;
using System.Text.Json;
using BankEs.Domain.BankAccount;
using BankEs.Domain.BankAccount.Events;
using EventStore.Client;

namespace BankEs.Infrastructure;

public class AccountAggregateRepository
{
    private readonly EventStoreClient _eventStoreClient;

    public AccountAggregateRepository(EventStoreClient eventStoreClient)
    {
        _eventStoreClient = eventStoreClient;
    }

    private string GetStreamName(AccountId accountId) => $"Account-{accountId.Id}";
    private string GetSnapshotName(AccountId accountId) => $"AccountSnapshot-{accountId.Id}";

    public async Task<Account> GetAsync(AccountId accountId, CancellationToken cancellationToken = default)
    {
        var snapshot = await LoadSnapshotAsync(accountId, cancellationToken);

        var account = snapshot == null ? new Account(accountId) : new Account(accountId, snapshot);

        var events = _eventStoreClient.ReadStreamAsync(
            Direction.Forwards,
            GetStreamName(accountId),
            snapshot == null ? StreamPosition.Start : StreamPosition.FromInt64(snapshot.Version + 1),
            cancellationToken: cancellationToken
        );

        if (await events.ReadState == ReadState.StreamNotFound)
            return account;

        await foreach (var @event in events)
        {
            var data = Encoding.UTF8.GetString(@event.Event.Data.ToArray());

            account.LoadChange(
                @event.OriginalEventNumber.ToInt64(),
                @event.Event.EventType switch
                {
                    nameof(AccountCreated) => JsonSerializer.Deserialize<AccountCreated>(data),
                    nameof(MoneyDeposited) => JsonSerializer.Deserialize<MoneyDeposited>(data),
                    nameof(MoneyWithdrawn) => JsonSerializer.Deserialize<MoneyWithdrawn>(data)
                });
        }

        return account;
    }

    public async Task SaveAsync(Account account, CancellationToken cancellationToken = default)
    {
        if (!account.GetChanges().Any())
            return;

        var changes = account.GetChanges()
            .Select(change => new EventData(
                eventId: Uuid.NewUuid(),
                type: change.GetType().Name,
                data: JsonSerializer.SerializeToUtf8Bytes(change)));
        
        var result = await _eventStoreClient.AppendToStreamAsync(
            GetStreamName(account.Id),
            StreamRevision.FromInt64(account.Version),
             //StreamState.Any,
            changes,
            cancellationToken: cancellationToken
        );
        
        account.ClearChanges();

        if (result.NextExpectedStreamRevision.ToInt64() % 5 == 0)
        {
            await AppendSnapshotAsync(account, result.NextExpectedStreamRevision.ToInt64(), cancellationToken);
        }
    }

    private async Task<AccountSnapshot?> LoadSnapshotAsync(AccountId accountId,
        CancellationToken cancellationToken = default)
    {
        var events = _eventStoreClient.ReadStreamAsync(
            Direction.Backwards,
            GetSnapshotName(accountId),
            StreamPosition.End,
            maxCount: 1,
            cancellationToken: cancellationToken
        );

        if (await events.ReadState == ReadState.StreamNotFound)
            return null;

        var lastEvent = await events.ElementAtAsync(0, cancellationToken: cancellationToken);

        return JsonSerializer.Deserialize<AccountSnapshot>(
            Encoding.UTF8.GetString(lastEvent.Event.Data.ToArray()));
    }

    private async Task AppendSnapshotAsync(Account account, long version, CancellationToken cancellationToken = default)
    {
        await _eventStoreClient.AppendToStreamAsync(
            GetSnapshotName(account.Id),
            StreamState.Any,
            new[]
            {
                new EventData(
                    Uuid.NewUuid(),
                    type: "snapshot",
                    data: JsonSerializer.SerializeToUtf8Bytes(new AccountSnapshot(account.State, version))
                )
            },
            cancellationToken: cancellationToken
        );
    }
}