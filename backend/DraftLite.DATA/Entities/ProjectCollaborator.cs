namespace DraftLite.Data.Entities;

public class ProjectCollaborator
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid RoleId { get; set; }
    public ProjectRole Role { get; set; } = null!;

    public Guid InvitedById { get; set; }
    public User InvitedBy { get; set; } = null!;
}
