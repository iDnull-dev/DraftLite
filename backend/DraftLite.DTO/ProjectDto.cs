namespace DraftLite.DTO;

public sealed class ProjectDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public Guid OwnerId { get; init; }
    public string OwnerPseudo { get; init; } = null!;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }
}

