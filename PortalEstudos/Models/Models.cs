namespace PortalEstudos.Models;

public class WeekModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "📚";
    public List<LessonModel> Lessons { get; set; } = new();
    public List<ExerciseModel> Exercises { get; set; } = new();
    public ChallengeModel? Challenge { get; set; }
}

public class LessonModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? CodeExample { get; set; }
    public string? CodeLanguage { get; set; }
    public int Order { get; set; }
}

public class ExerciseModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string InitialCode { get; set; } = string.Empty;
    public string ExpectedOutput { get; set; } = string.Empty;
    public ExerciseDifficulty Difficulty { get; set; }
    public List<string> Hints { get; set; } = new();
    public string? Solution { get; set; }
}

public enum ExerciseDifficulty
{
    Easy,
    Medium,
    Hard
}

public class ChallengeModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GitHubUrl { get; set; } = string.Empty;
    public List<string> Requirements { get; set; } = new();
}

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

public class DashboardStats
{
    public int TotalWeeks { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public int CompletedExercises { get; set; }
    public int TotalExercises { get; set; }
    public int CompletedChallenges { get; set; }
    public int TotalChallenges { get; set; }
    public int CurrentStreak { get; set; }
    public double ProgressPercentage { get; set; }
}
