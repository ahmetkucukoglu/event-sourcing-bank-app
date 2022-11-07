using System.Text;
using EventStore.Client;

namespace BankEs.Api;

public class AccountAggregateSubscriber : IHostedService
{
    private readonly EventStoreClient _eventStoreClient;
    private StreamSubscription _subscription;

    public AccountAggregateSubscriber(EventStoreClient eventStoreClient)
    {
        _eventStoreClient = eventStoreClient;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var filter = new SubscriptionFilterOptions(StreamFilter.Prefix("Account-"));

        var checkpoint = GetCheckpoint();

        _subscription = await _eventStoreClient.SubscribeToAllAsync(
            checkpoint == null ? FromAll.Start : FromAll.After(checkpoint.Value),
            (_, @event, _) =>
            {
                var data = Encoding.UTF8.GetString(@event.Event.Data.ToArray());
                Console.WriteLine(data);
                
                SaveCheckpoint(@event.Event.Position);

                return Task.CompletedTask;
            },
            filterOptions: filter,
            cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _subscription.Dispose();

        return Task.CompletedTask;
    }

    private void SaveCheckpoint(Position position)
    {
        using var sw = File.CreateText("checkpoint.txt");
        sw.WriteLine("{0}:{1}", position.CommitPosition, position.PreparePosition);
    }

    private Position? GetCheckpoint()
    {
        using var sr = File.OpenText("checkpoint.txt");
        var position = sr.ReadToEnd().Split(":", StringSplitOptions.RemoveEmptyEntries);

        if (position.Length == 0)
            return null;

        return new Position(Convert.ToUInt64(position[0]), Convert.ToUInt64(position[1]));
    }
}