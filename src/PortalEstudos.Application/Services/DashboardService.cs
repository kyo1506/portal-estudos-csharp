using PortalEstudos.Application.Abstractions;
using PortalEstudos.Application.Dtos;
using PortalEstudos.Domain.Entities;

namespace PortalEstudos.Application.Services;

public interface IDashboardService
{
    DashboardStats GetStats(UserProgress progress);
    double GetWeekProgress(Week week, UserProgress progress);
    bool IsWeekCompleted(Week week, UserProgress progress);
    bool IsWeekInProgress(Week week, UserProgress progress);
}

/// <summary>Cálculo de progresso e estatísticas do painel.</summary>
public sealed class DashboardService : IDashboardService
{
    private readonly IContentRepository _content;

    public DashboardService(IContentRepository content)
    {
        _content = content;
    }

    public DashboardStats GetStats(UserProgress progress)
    {
        var weeks = _content.GetAllWeeks();

        var totalLessons = weeks.Sum(w => w.Lessons.Count);
        var totalExercises = weeks.Sum(w => w.Exercises.Count);
        var totalChallenges = weeks.Count(w => w.Challenge != null);

        var done = progress.CompletedLessons.Count + progress.CompletedExercises.Count;
        var total = totalLessons + totalExercises;

        return new DashboardStats
        {
            TotalWeeks = weeks.Count,
            CompletedLessons = progress.CompletedLessons.Count,
            TotalLessons = totalLessons,
            CompletedExercises = progress.CompletedExercises.Count,
            TotalExercises = totalExercises,
            CompletedChallenges = progress.CompletedChallenges.Count,
            TotalChallenges = totalChallenges,
            CurrentStreak = progress.CurrentStreak,
            ProgressPercentage = total > 0
                ? Math.Round((double)done / total * 100, 1)
                : 0
        };
    }

    public double GetWeekProgress(Week week, UserProgress progress)
    {
        var totalItems = week.Lessons.Count + week.Exercises.Count + (week.Challenge != null ? 1 : 0);
        if (totalItems == 0) return 0;

        var completed = week.Lessons.Count(l => progress.CompletedLessons.Contains(l.Id))
                      + week.Exercises.Count(e => progress.CompletedExercises.Contains(e.Id))
                      + (week.Challenge != null && progress.CompletedChallenges.Contains(week.Challenge.Id) ? 1 : 0);

        return Math.Round((double)completed / totalItems * 100, 1);
    }

    public bool IsWeekCompleted(Week week, UserProgress progress)
    {
        var lessonsDone = week.Lessons.All(l => progress.CompletedLessons.Contains(l.Id));
        var exercisesDone = week.Exercises.All(e => progress.CompletedExercises.Contains(e.Id));
        var challengeDone = week.Challenge == null || progress.CompletedChallenges.Contains(week.Challenge.Id);
        return lessonsDone && exercisesDone && challengeDone;
    }

    public bool IsWeekInProgress(Week week, UserProgress progress)
    {
        var anyLesson = week.Lessons.Any(l => progress.CompletedLessons.Contains(l.Id));
        var anyExercise = week.Exercises.Any(e => progress.CompletedExercises.Contains(e.Id));
        var anyChallenge = week.Challenge != null && progress.CompletedChallenges.Contains(week.Challenge.Id);
        return anyLesson || anyExercise || anyChallenge;
    }
}
