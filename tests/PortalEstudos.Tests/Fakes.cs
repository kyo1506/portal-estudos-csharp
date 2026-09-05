using PortalEstudos.Application.Abstractions;
using PortalEstudos.Domain.Entities;
using PortalEstudos.Domain.Enums;

namespace PortalEstudos.Tests;

/// <summary>Repositório de conteúdo em memória para testes determinísticos.</summary>
internal sealed class FakeContentRepository : IContentRepository
{
    private readonly List<Fase> _fases;

    public FakeContentRepository(params Fase[] fases) => _fases = fases.ToList();

    public IReadOnlyList<Fase> GetAllFases() => _fases;

    public Fase? GetFase(int id) => _fases.FirstOrDefault(w => w.Id == id);

    public LessonModel? GetLesson(int faseId, int lessonId)
        => GetFase(faseId)?.Lessons.FirstOrDefault(l => l.Id == lessonId);

    public ExerciseModel? GetExercise(int faseId, int exerciseId)
        => GetFase(faseId)?.Exercises.FirstOrDefault(e => e.Id == exerciseId);
}

/// <summary>Armazena o progresso em memória (sem interop JS).</summary>
internal sealed class InMemoryProgressStore : IProgressStore
{
    public UserProgress Stored { get; set; } = new();
    public int SaveCount { get; private set; }

    public Task<UserProgress> LoadAsync() => Task.FromResult(Stored);

    public Task SaveAsync(UserProgress progress)
    {
        Stored = progress;
        SaveCount++;
        return Task.CompletedTask;
    }

    public Task ResetAsync()
    {
        Stored = new UserProgress();
        return Task.CompletedTask;
    }
}

internal static class TestData
{
    public static Fase BuildWeek(int id = 1, int lessonCount = 2, int exerciseCount = 2, bool hasChallenge = true) => new()
    {
        Id = id,
        Title = $"Semana {id}",
        Description = "Descrição",
        Icon = "📚",
        Lessons = Enumerable.Range(1, lessonCount).Select(l => new LessonModel
        {
            Id = l,
            Title = $"Lição {l}",
            Content = $"# Título\nConteúdo {l}.",
            CodeExample = "Console.WriteLine();",
            CodeLanguage = "csharp",
            Order = l
        }).ToList(),
        Exercises = Enumerable.Range(1, exerciseCount).Select(e => new ExerciseModel
        {
            Id = e,
            Title = $"Exercício {e}",
            Description = "Descrição",
            InitialCode = "using System;",
            ExpectedOutput = "ok",
            Difficulty = e % 2 == 0 ? ExerciseDifficulty.Medium : ExerciseDifficulty.Easy,
            Hints = new List<string> { "dica" },
            Solution = "Console.WriteLine();"
        }).ToList(),
        Challenge = hasChallenge ? new Challenge
        {
            Id = id,
            Title = $"Desafio {id}",
            Description = "Descrição",
            GitHubUrl = "https://github.com/x",
            Requirements = new List<string> { "req" }
        } : null
    };
}
