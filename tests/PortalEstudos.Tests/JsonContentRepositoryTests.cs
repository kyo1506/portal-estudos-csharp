using PortalEstudos.Domain.Enums;
using PortalEstudos.Infrastructure.Content;
using Xunit;

namespace PortalEstudos.Tests;

public class JsonContentRepositoryTests
{
    private readonly JsonContentRepository _repository = new();

    [Fact]
    public void GetAllFases_LoadsFaseZero()
    {
        var fases = _repository.GetAllFases();
        Assert.Single(fases);
        Assert.Equal(0, fases[0].Id);
    }

    [Fact]
    public void FaseZero_ContemLicoesEExerciciosSemDesafio()
    {
        var fase = _repository.GetFase(0);

        Assert.NotNull(fase);
        Assert.Equal(11, fase!.Lessons.Count);      // teoria completa da Fase 00
        Assert.Equal(6, fase.Exercises.Count);
        Assert.Null(fase.Challenge);                // Fase 00 não tem desafio externo
    }

    [Fact]
    public void Difficulty_IsMappedToEnum()
    {
        var fase = _repository.GetFase(0);
        var easy = fase!.Exercises.First(e => e.Id == 1);
        var medium = fase.Exercises.First(e => e.Id == 3);

        Assert.Equal(ExerciseDifficulty.Easy, easy.Difficulty);
        Assert.Equal(ExerciseDifficulty.Medium, medium.Difficulty);
    }

    [Fact]
    public void GetLesson_ReturnsLessonById()
    {
        var lesson = _repository.GetLesson(0, 1);
        Assert.NotNull(lesson);
        Assert.Contains("Programar", lesson!.Title);
    }

    [Fact]
    public void GetExercise_ReturnsExerciseById()
    {
        var exercise = _repository.GetExercise(0, 6);
        Assert.NotNull(exercise);
        Assert.Equal("Soma dos dígitos", exercise!.Title);
    }

    [Fact]
    public void GetFase_UnknownId_ReturnsNull()
    {
        Assert.Null(_repository.GetFase(999));
    }

    [Fact]
    public void Lessons_KeepOrderField()
    {
        var fase = _repository.GetFase(0);
        Assert.Equal(Enumerable.Range(1, 11), fase!.Lessons.Select(l => l.Order));
    }
}
