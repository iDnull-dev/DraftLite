namespace DraftLite.Data.Entities;

public class AuditLogEntry
{
    public Guid Id { get; set; }

    public string EntityType { get; set; } = null!;
    public Guid EntityId { get; set; }

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public string Action { get; set; } = null!;
    public DateTimeOffset ChangedAt { get; set; }
}
