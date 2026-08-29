namespace PortalEstudos.Domain.Entities;

public class Challenge
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string GitHubUrl { get; init; } = string.Empty;
    public List<string> Requirements { get; init; } = new();
}
