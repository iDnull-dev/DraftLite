namespace DraftLite.Data.Entities;

public class ProjectRole
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;

    public ICollection<ProjectCollaborator> ProjectCollaborators { get; set; } = new List<ProjectCollaborator>();
}
