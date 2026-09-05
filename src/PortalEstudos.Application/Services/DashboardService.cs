using PortalEstudos.Application.Abstractions;
using PortalEstudos.Application.Dtos;
using PortalEstudos.Domain.Entities;

namespace PortalEstudos.Application.Services;

public interface IDashboardService
{
    DashboardStats GetStats(UserProgress progress);
    double GetFaseProgress(Fase fase, UserProgress progress);
    bool IsFaseCompleted(Fase fase, UserProgress progress);
    bool IsFaseInProgress(Fase fase, UserProgress progress);
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
        var fases = _content.GetAllFases();

        var totalLessons = fases.Sum(w => w.Lessons.Count);
        var totalExercises = fases.Sum(w => w.Exercises.Count);
        var totalChallenges = fases.Count(w => w.Challenge != null);

        var done = progress.CompletedLessons.Count + progress.CompletedExercises.Count;
        var total = totalLessons + totalExercises;

        return new DashboardStats
        {
            TotalFases = fases.Count,
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

    public double GetFaseProgress(Fase fase, UserProgress progress)
    {
        var totalItems = fase.Lessons.Count + fase.Exercises.Count + (fase.Challenge != null ? 1 : 0);
        if (totalItems == 0) return 0;

        var completed = fase.Lessons.Count(l => progress.CompletedLessons.Contains(l.Id))
                      + fase.Exercises.Count(e => progress.CompletedExercises.Contains(e.Id))
                      + (fase.Challenge != null && progress.CompletedChallenges.Contains(fase.Challenge.Id) ? 1 : 0);

        return Math.Round((double)completed / totalItems * 100, 1);
    }

    public bool IsFaseCompleted(Fase fase, UserProgress progress)
    {
        var lessonsDone = fase.Lessons.All(l => progress.CompletedLessons.Contains(l.Id));
        var exercisesDone = fase.Exercises.All(e => progress.CompletedExercises.Contains(e.Id));
        var challengeDone = fase.Challenge == null || progress.CompletedChallenges.Contains(fase.Challenge.Id);
        return lessonsDone && exercisesDone && challengeDone;
    }

    public bool IsFaseInProgress(Fase fase, UserProgress progress)
    {
        var anyLesson = fase.Lessons.Any(l => progress.CompletedLessons.Contains(l.Id));
        var anyExercise = fase.Exercises.Any(e => progress.CompletedExercises.Contains(e.Id));
        var anyChallenge = fase.Challenge != null && progress.CompletedChallenges.Contains(fase.Challenge.Id);
        return anyLesson || anyExercise || anyChallenge;
    }
}
