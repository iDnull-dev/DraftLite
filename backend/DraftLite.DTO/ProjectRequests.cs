namespace DraftLite.DTO;

public sealed class CreateProjectRequest
{
    public string Title { get; init; } = null!;
}

public sealed class UpdateProjectRequest
{
    public string Title { get; init; } = null!;
}

