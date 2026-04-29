namespace DraftLite.Data.Entities;

public class Project
{
    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public string Title { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<Page> Pages { get; set; } = new List<Page>();
    public ICollection<ProjectCollaborator> Collaborators { get; set; } = new List<ProjectCollaborator>();
    public ICollection<ProjectHistory> HistoryEntries { get; set; } = new List<ProjectHistory>();
}
