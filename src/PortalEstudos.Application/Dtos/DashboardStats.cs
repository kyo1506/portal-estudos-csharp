namespace PortalEstudos.Application.Dtos;

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
