using System.Text;
using Azure.Messaging.EventHubs.Consumer;

namespace SmartCityBackend.Features.EventHub;

public class EventHubListener : IHostedService
{
    private readonly EventHubConsumerClient _consumerClient;
    private long lastCheckpointSequenceNumber = 0;

    public EventHubListener(IConfiguration configuration)
    {
        var eventHubConnectionString = configuration.GetSection("EventHub:ConnectionString").Value!;
        var eventHubName = configuration.GetSection("EventHub.Name").Value!;
        _consumerClient = new EventHubConsumerClient(EventHubConsumerClient.DefaultConsumerGroupName,
            eventHubConnectionString, eventHubName);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var latestEventPosition = EventPosition.FromSequenceNumber(lastCheckpointSequenceNumber);
        
        await foreach (PartitionEvent partitionEvent in _consumerClient.ReadEventsAsync(cancellationToken: cancellationToken))
        {
            Console.WriteLine($"Message received on partition {partitionEvent.Partition.ReadLastEnqueuedEventProperties()}:");
            Console.WriteLine(Encoding.UTF8.GetString(partitionEvent.Data.Body.ToArray()));
            Console.WriteLine(partitionEvent.Partition);
            
            lastCheckpointSequenceNumber = partitionEvent.Data.SequenceNumber;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _consumerClient.CloseAsync(cancellationToken);
    }
}