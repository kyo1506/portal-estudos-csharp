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
        Assert.Equal(8, fases.Count);
        Assert.Contains(fases, f => f.Id == 0);
        Assert.Contains(fases, f => f.Id == 1);
        Assert.Contains(fases, f => f.Id == 2);
        Assert.Contains(fases, f => f.Id == 3);
        Assert.Contains(fases, f => f.Id == 4);
        Assert.Contains(fases, f => f.Id == 5);
        Assert.Contains(fases, f => f.Id == 6);
        Assert.Contains(fases, f => f.Id == 7);
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
    public void FaseDois_ContemLicoesEExercicios()
    {
        var fase = _repository.GetFase(2);

        Assert.NotNull(fase);
        Assert.Equal(6, fase!.Lessons.Count);       // 6 aulas da teoria da Fase 02
        Assert.Equal(6, fase.Exercises.Count);
        Assert.Contains("Orientada a Objetos", fase.Title);
    }

    [Fact]
    public void FaseTres_ContemLicoesEExercicios()
    {
        var fase = _repository.GetFase(3);

        Assert.NotNull(fase);
        Assert.Equal(6, fase!.Lessons.Count);       // 6 aulas da teoria da Fase 03
        Assert.Equal(6, fase.Exercises.Count);
        Assert.Contains("Coleções", fase.Title);
    }

    [Fact]
    public void FaseQuatro_ContemLicoesEExercicios()
    {
        var fase = _repository.GetFase(4);

        Assert.NotNull(fase);
        Assert.Equal(7, fase!.Lessons.Count);       // 7 aulas da teoria da Fase 04
        Assert.Equal(6, fase.Exercises.Count);
        Assert.Contains("Runtime", fase.Title);
    }

    [Fact]
    public void FaseCinco_ContemLicoesEExercicios()
    {
        var fase = _repository.GetFase(5);

        Assert.NotNull(fase);
        Assert.Equal(5, fase!.Lessons.Count);       // 5 aulas da teoria da Fase 05
        Assert.Equal(6, fase.Exercises.Count);
        Assert.Contains("Arquitetura", fase.Title);
    }

    [Fact]
    public void FaseSeis_ContemLicoesEExercicios()
    {
        var fase = _repository.GetFase(6);

        Assert.NotNull(fase);
        Assert.Equal(6, fase!.Lessons.Count);       // 6 aulas da teoria da Fase 06
        Assert.Equal(6, fase.Exercises.Count);
        Assert.Contains("Web", fase.Title);
    }

    [Fact]
    public void FaseSete_ContemLicoesEExercicios()
    {
        var fase = _repository.GetFase(7);

        Assert.NotNull(fase);
        Assert.Equal(5, fase!.Lessons.Count);       // 5 aulas da teoria da Fase 07
        Assert.Equal(6, fase.Exercises.Count);
        Assert.Contains("Performance", fase.Title);
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
