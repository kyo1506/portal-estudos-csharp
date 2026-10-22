using System.Text.Json;
using PortalEstudos.Domain.Entities;
using PortalEstudos.Infrastructure.Progress;
using Xunit;

namespace PortalEstudos.Tests;

public class LocalStorageProgressStoreTests
{
    [Fact]
    public async Task LoadAsync_DuringSsr_ReturnsEmptyProgressGracefully()
    {
        var js = new FakeJSRuntime { SimulateSsr = true };
        var store = new LocalStorageProgressStore(js);

        var progress = await store.LoadAsync();

        Assert.NotNull(progress);
        Assert.Empty(progress.CompletedLessons);
        Assert.Empty(progress.CompletedExercises);
    }

    [Fact]
    public async Task LoadAsync_WhenStored_DeserializesCorrectly()
    {
        var js = new FakeJSRuntime();
        var original = new UserProgress
        {
            CompletedLessons = { 1, 2, 3 },
            CompletedExercises = { 10 },
            ExerciseAnswers = { [10] = "Console.WriteLine(1);" }
        };
        js.Storage["portal-estudos-progress"] = JsonSerializer.Serialize(original);

        var store = new LocalStorageProgressStore(js);
        var loaded = await store.LoadAsync();

        Assert.Equal(3, loaded.CompletedLessons.Count);
        Assert.Contains(10, loaded.CompletedExercises);
        Assert.Equal("Console.WriteLine(1);", loaded.ExerciseAnswers[10]);
    }

    [Fact]
    public async Task SaveAsync_PersistsJsonInStorage()
    {
        var js = new FakeJSRuntime();
        var store = new LocalStorageProgressStore(js);
        var progress = new UserProgress
        {
            CompletedLessons = { 5 },
            CompletedExercises = { 20 }
        };

        await store.SaveAsync(progress);

        Assert.True(js.Storage.ContainsKey("portal-estudos-progress"));
        var json = js.Storage["portal-estudos-progress"];
        Assert.NotNull(json);
        Assert.Contains("\"CompletedLessons\":[5]", json);
    }

    [Fact]
    public async Task ResetAsync_RemovesKeyFromStorage()
    {
        var js = new FakeJSRuntime();
        js.Storage["portal-estudos-progress"] = "{}";
        var store = new LocalStorageProgressStore(js);

        await store.ResetAsync();

        Assert.False(js.Storage.ContainsKey("portal-estudos-progress"));
    }
}
