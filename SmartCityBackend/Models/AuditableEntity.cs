namespace SmartCityBackend.Models;

public abstract class AuditableEntity<TId>
{
    public TId Id { get; set; } = default!;

    public DateTime CreatedAtUtc { get; } = DateTime.UtcNow;
}

public abstract class AuditableEntity : AuditableEntity<long>
{
}