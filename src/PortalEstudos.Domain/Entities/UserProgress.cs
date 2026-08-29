namespace PortalEstudos.Domain.Entities;

/// <summary>Progresso de um aluno no curso. As chaves dos conjuntos são os IDs
/// globais das lições/exercícios/desafios (persistido no navegador).</summary>
public class UserProgress
{
    public int UserId { get; set; }
    public HashSet<int> CompletedLessons { get; set; } = new();
    public HashSet<int> CompletedExercises { get; set; } = new();
    public HashSet<int> CompletedChallenges { get; set; } = new();
    public Dictionary<int, string> ExerciseAnswers { get; set; } = new();
    public DateTime LastActivity { get; set; } = DateTime.Now;
    public int CurrentStreak { get; set; } = 1;
}
