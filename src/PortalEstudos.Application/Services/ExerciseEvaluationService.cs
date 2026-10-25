namespace PortalEstudos.Application.Services;

using System.Text.RegularExpressions;
using PortalEstudos.Domain.Entities;

public sealed record ExerciseEvaluationResult(
    bool IsCorrect,
    string ActualOutput,
    string? ErrorMessage = null,
    string? CompilerErrors = null
);

public interface IExerciseEvaluationService
{
    Task<ExerciseEvaluationResult> EvaluateAsync(string userCode, ExerciseModel exercise, CancellationToken ct = default);
    bool IsCorrect(string userCode, ExerciseModel exercise);
}

public partial class ExerciseEvaluationService : IExerciseEvaluationService
{
    public virtual Task<ExerciseEvaluationResult> EvaluateAsync(string userCode, ExerciseModel exercise, CancellationToken ct = default)
    {
        var correct = IsCorrect(userCode, exercise);
        return Task.FromResult(new ExerciseEvaluationResult(
            correct,
            correct ? exercise.ExpectedOutput : string.Empty,
            correct ? null : "Código não atende aos requisitos esperados."
        ));
    }

    public virtual bool IsCorrect(string userCode, ExerciseModel exercise)
    {
        if (string.IsNullOrWhiteSpace(userCode)) return false;
        if (string.IsNullOrWhiteSpace(exercise.Solution)) return userCode.Contains("Console.WriteLine");

        var userNorm = NormalizeCode(userCode);
        var solutionNorm = NormalizeCode(exercise.Solution);

        if (userNorm == solutionNorm) return true;

        var hasOutput = userCode.Contains("Console.WriteLine");
        var solutionTokens = ExtractTokens(solutionNorm);
        var matched = solutionTokens.Count > 0
            && solutionTokens.Count(token => userNorm.Contains(token)) >= Math.Ceiling(solutionTokens.Count * 0.6);

        return hasOutput && matched;
    }

    public static List<string> ExtractTokens(string normalizedCode)
    {
        return IdentifierRegex().Matches(normalizedCode)
            .Select(m => m.Value)
            .Distinct()
            .ToList();
    }

    public static string NormalizeCode(string code)
    {
        var noComments = LineCommentRegex().Replace(code, "");
        noComments = BlockCommentRegex().Replace(noComments, "");
        noComments = StringLiteralRegex().Replace(noComments, "\"\"");
        return WhitespaceRegex().Replace(noComments, " ").Trim();
    }

    [GeneratedRegex(@"[A-Za-z_][A-Za-z0-9_]{3,}")]
    private static partial Regex IdentifierRegex();

    [GeneratedRegex(@"//.*")]
    private static partial Regex LineCommentRegex();

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex BlockCommentRegex();

    [GeneratedRegex("\"[^\"]*\"")]
    private static partial Regex StringLiteralRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
