using PortalEstudos.Application.Services;
using PortalEstudos.Domain.Entities;
using Xunit;

namespace PortalEstudos.Tests;

public class ProgressServiceTests
{
    [Fact]
    public async Task MarkLessonComplete_AddsAndSaves()
    {
        var store = new InMemoryProgressStore();
        var service = new ProgressService(store);

        await service.MarkLessonCompleteAsync(7);

        Assert.Contains(7, store.Stored.CompletedLessons);
        Assert.Equal(1, store.SaveCount);
        Assert.True(store.Stored.LastActivity > DateTime.MinValue);
    }

    [Fact]
    public async Task MarkLessonIncomplete_Removes()
    {
        var store = new InMemoryProgressStore { Stored = new UserProgress { CompletedLessons = { 3 } } };
        var service = new ProgressService(store);

        await service.MarkLessonIncompleteAsync(3);

        Assert.DoesNotContain(3, store.Stored.CompletedLessons);
    }

    [Fact]
    public async Task MarkExerciseComplete_StoresAnswer()
    {
        var store = new InMemoryProgressStore();
        var service = new ProgressService(store);

        await service.MarkExerciseCompleteAsync(4, "Console.WriteLine();");

        Assert.Contains(4, store.Stored.CompletedExercises);
        Assert.Equal("Console.WriteLine();", store.Stored.ExerciseAnswers[4]);
    }

    [Fact]
    public async Task MarkChallengeComplete_Adds()
    {
        var store = new InMemoryProgressStore();
        var service = new ProgressService(store);

        await service.MarkChallengeCompleteAsync(9);

        Assert.Contains(9, store.Stored.CompletedChallenges);
    }

    [Fact]
    public async Task LoadAsync_CachesSameInstance()
    {
        var store = new InMemoryProgressStore();
        var service = new ProgressService(store);

        var first = await service.LoadAsync();
        var second = await service.LoadAsync();

        Assert.Same(first, second);
        Assert.Equal(0, store.SaveCount); // cache em memória evita leituras/gravações repetidas
    }

    [Fact]
    public async Task Reset_ClearsAndInvalidatesCache()
    {
        var store = new InMemoryProgressStore();
        var service = new ProgressService(store);
        await service.MarkLessonCompleteAsync(1);

        await service.ResetAsync();

        var after = await service.LoadAsync();
        Assert.Empty(after.CompletedLessons);
    }
}
