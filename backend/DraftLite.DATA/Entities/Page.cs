namespace DraftLite.Data.Entities;

public class Page
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string Title { get; set; } = null!;
    public string Blocks { get; set; } = "[]";
    public int OrderIndex { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<ProjectHistory> HistoryEntries { get; set; } = new List<ProjectHistory>();
}
