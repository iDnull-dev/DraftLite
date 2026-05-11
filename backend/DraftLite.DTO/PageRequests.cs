namespace DraftLite.DTO;

public sealed class CreatePageRequest
{
    public string Title { get; init; } = null!;
}

public sealed class UpdatePageRequest
{
    public string Title { get; init; } = null!;
    public string Blocks { get; init; } = "[]";
}

