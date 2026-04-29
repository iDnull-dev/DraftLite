namespace DraftLite.Data.Entities;

public class User
{
    public Guid Id { get; set; }

    public string? GoogleId { get; set; }
    public string Email { get; set; } = null!;
    public string Pseudo { get; set; } = null!;
    public string Theme { get; set; } = "light";

    public DateTimeOffset CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTimeOffset? BanAt { get; set; }
    public string? BanReason { get; set; }

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public ICollection<Project> OwnedProjects { get; set; } = new List<Project>();
    public ICollection<ProjectCollaborator> ProjectCollaborations { get; set; } = new List<ProjectCollaborator>();

    public ICollection<ProjectCollaborator> SentProjectInvites { get; set; } = new List<ProjectCollaborator>();
    public ICollection<ProjectHistory> ProjectHistoryEntries { get; set; } = new List<ProjectHistory>();
    public ICollection<AuditLogEntry> AuditLogEntries { get; set; } = new List<AuditLogEntry>();
}
