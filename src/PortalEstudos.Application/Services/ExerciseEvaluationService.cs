using System.Text.RegularExpressions;
using PortalEstudos.Domain.Entities;

namespace PortalEstudos.Application.Services;

public interface IExerciseEvaluationService
{
    bool IsCorrect(string userCode, ExerciseModel exercise);
}

/// <summary>Avalia o código do aluno contra a solução esperada (lógica pura, testável).</summary>
public sealed partial class ExerciseEvaluationService : IExerciseEvaluationService
{
    public bool IsCorrect(string userCode, ExerciseModel exercise)
    {
        if (string.IsNullOrWhiteSpace(userCode)) return false;
        if (string.IsNullOrWhiteSpace(exercise.Solution)) return userCode.Contains("Console.WriteLine");

        var userNorm = NormalizeCode(userCode);
        var solutionNorm = NormalizeCode(exercise.Solution);

        if (userNorm == solutionNorm) return true;

        // Solução usada como template: aceita se o aluno produziu saída e trouxe a lógica central.
        var hasOutput = userCode.Contains("Console.WriteLine");
        var solutionTokens = ExtractTokens(solutionNorm);
        var matched = solutionTokens.Count > 0
            && solutionTokens.Count(token => userNorm.Contains(token)) >= Math.Ceiling(solutionTokens.Count * 0.6);

        return hasOutput && matched;
    }

    internal static List<string> ExtractTokens(string normalizedCode)
    {
        // Palavras com 4+ chars (identificadores/keywords relevantes), ignorando pontuação.
        return IdentifierRegex().Matches(normalizedCode)
            .Select(m => m.Value)
            .Distinct()
            .ToList();
    }

    internal static string NormalizeCode(string code)
    {
        var noComments = LineCommentRegex().Replace(code, "");
        noComments = BlockCommentRegex().Replace(noComments, "");
        // Remove strings literais (para não penalizar textos diferentes).
        noComments = StringLiteralRegex().Replace(noComments, "\"\"");
        // Colapsa espaços/quebras.
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
