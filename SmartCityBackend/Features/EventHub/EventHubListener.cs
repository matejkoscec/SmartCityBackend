using System.Text;
using Azure.Messaging.EventHubs.Consumer;
using Microsoft.EntityFrameworkCore;
using SmartCityBackend.Infrastructure.Persistence;
using SmartCityBackend.Models;

namespace SmartCityBackend.Features.EventHub;

public class EventHubListener : IHostedService
{
    private readonly EventHubConsumerClient _consumerClient;
    private readonly DatabaseContext _dbContext;
    private readonly string _partitionId;

    public EventHubListener(IConfiguration configuration, IServiceScopeFactory iServiceScopeFactory)
    {
        var eventHubConnectionString = configuration.GetSection("EventHub:ConnectionString").Value!;
        var eventHubName = configuration.GetSection("EventHub.Name").Value!;
        _consumerClient = new EventHubConsumerClient(EventHubConsumerClient.DefaultConsumerGroupName,
            eventHubConnectionString, eventHubName);
        _dbContext = iServiceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<DatabaseContext>();
        _partitionId = "0";
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // get offset sorted by descending date
        var offset = await _dbContext.EventHubInfo
            .OrderByDescending(e => e.CreatedAtUtc)
            .Select(e => e.Offset)
            .FirstOrDefaultAsync(cancellationToken);

        await StartProcessingEvents(offset, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _consumerClient.CloseAsync(cancellationToken);
    }

    private async Task StartProcessingEvents(long offset, CancellationToken cancellationToken)
    {
        var eventPosition = EventPosition.FromOffset(offset);

        await foreach (PartitionEvent partitionEvent in _consumerClient.ReadEventsFromPartitionAsync(_partitionId,
                           eventPosition, cancellationToken))
        {
            Console.WriteLine(Encoding.UTF8.GetString(partitionEvent.Data.Body.ToArray()));
            Console.WriteLine("Enqueued at: " +
                              partitionEvent.Partition.ReadLastEnqueuedEventProperties().EnqueuedTime);
            Console.WriteLine(partitionEvent.Data.Offset);
            await PersistOffset(partitionEvent.Data.Offset);
        }
    }

    private async Task PersistOffset(long offset)
    {
        _dbContext.EventHubInfo.Add(new EventHubInfo { Offset = offset });
        await _dbContext.SaveChangesAsync();
    }
}