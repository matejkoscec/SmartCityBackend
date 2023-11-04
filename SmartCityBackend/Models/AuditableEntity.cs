namespace SmartCityBackend.Models;

public abstract class AuditableEntity<TId>
{
    public TId Id { get; set; } = default!;

    public DateTimeOffset CreatedAtUtc { get; } = DateTimeOffset.UtcNow;
}

public abstract class AuditableEntity : AuditableEntity<long>
{
}