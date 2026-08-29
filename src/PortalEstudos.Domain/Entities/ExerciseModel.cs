using PortalEstudos.Domain.Enums;

namespace PortalEstudos.Domain.Entities;

public class ExerciseModel
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string InitialCode { get; init; } = string.Empty;
    public string ExpectedOutput { get; init; } = string.Empty;
    public ExerciseDifficulty Difficulty { get; init; }
    public List<string> Hints { get; init; } = new();
    public string? Solution { get; init; }
}
