namespace SmartCityBackend.Models;

public class EventHubInfo: AuditableEntity
{
    public long Offset { get; set; }
}