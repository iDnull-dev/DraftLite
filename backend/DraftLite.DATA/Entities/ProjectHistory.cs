namespace DraftLite.Data.Entities;

public class ProjectHistory
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid PageId { get; set; }
    public Page Page { get; set; } = null!;

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public string Action { get; set; } = null!;

    public int BaseVersion { get; set; }
    public int Version { get; set; }

    public string Patch { get; set; } = "[]";

    public DateTimeOffset CreatedAt { get; set; }
}
