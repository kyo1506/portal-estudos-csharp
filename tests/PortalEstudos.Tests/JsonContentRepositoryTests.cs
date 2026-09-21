using PortalEstudos.Domain.Enums;
using PortalEstudos.Infrastructure.Content;
using Xunit;

namespace PortalEstudos.Tests;

public class JsonContentRepositoryTests
{
    private readonly JsonContentRepository _repository = new();

    [Fact]
    public void GetAllFases_LoadsFasesCadastradas()
    {
        var fases = _repository.GetAllFases();
        Assert.Equal(2, fases.Count);
        Assert.Contains(fases, f => f.Id == 0);
        Assert.Contains(fases, f => f.Id == 1);
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
    public void FaseUm_ContemLicoesEExercicios()
    {
        var fase = _repository.GetFase(1);

        Assert.NotNull(fase);
        Assert.Equal(10, fase!.Lessons.Count);      // 10 aulas da teoria da Fase 01
        Assert.Equal(6, fase.Exercises.Count);
        Assert.Contains("C# Básico", fase.Title);
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

        var lessonFase1 = _repository.GetLesson(1, 1);
        Assert.NotNull(lessonFase1);
        Assert.Contains("Valor", lessonFase1!.Title);
    }

    [Fact]
    public void GetExercise_ReturnsExerciseById()
    {
        var exercise = _repository.GetExercise(0, 6);
        Assert.NotNull(exercise);
        Assert.Equal("Soma dos dígitos", exercise!.Title);

        var exerciseFase1 = _repository.GetExercise(1, 1);
        Assert.NotNull(exerciseFase1);
        Assert.Contains("Dinheiro", exerciseFase1!.Title);
    }

    [Fact]
    public void GetFase_UnknownId_ReturnsNull()
    {
        Assert.Null(_repository.GetFase(999));
    }

    [Fact]
    public void Lessons_KeepOrderField()
    {
        var fase0 = _repository.GetFase(0);
        Assert.Equal(Enumerable.Range(1, 11), fase0!.Lessons.Select(l => l.Order));

        var fase1 = _repository.GetFase(1);
        Assert.Equal(Enumerable.Range(1, 10), fase1!.Lessons.Select(l => l.Order));
    }
}
