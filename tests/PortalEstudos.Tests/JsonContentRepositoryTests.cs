using PortalEstudos.Domain.Enums;
using PortalEstudos.Infrastructure.Content;
using Xunit;

namespace PortalEstudos.Tests;

public class JsonContentRepositoryTests
{
    private readonly JsonContentRepository _repository = new();

    [Fact]
    public void GetAllWeeks_LoadsFourWeeks()
    {
        var weeks = _repository.GetAllWeeks();
        Assert.Equal(4, weeks.Count);
    }

    [Fact]
    public void WeekOne_ContainsLessonsExercisesAndChallenge()
    {
        var week = _repository.GetWeek(1);

        Assert.NotNull(week);
        Assert.Equal(3, week!.Lessons.Count);
        Assert.Equal(3, week.Exercises.Count);
        Assert.NotNull(week.Challenge);
    }

    [Fact]
    public void Difficulty_IsMappedToEnum()
    {
        var week = _repository.GetWeek(1);
        var easy = week!.Exercises.First(e => e.Id == 1);
        var medium = week.Exercises.First(e => e.Id == 3);

        Assert.Equal(ExerciseDifficulty.Easy, easy.Difficulty);
        Assert.Equal(ExerciseDifficulty.Medium, medium.Difficulty);
    }

    [Fact]
    public void GetLesson_ReturnsLessonById()
    {
        var lesson = _repository.GetLesson(2, 1);
        Assert.NotNull(lesson);
        Assert.Equal("Os 4 Pilares da OO", lesson!.Title);
    }

    [Fact]
    public void GetExercise_ReturnsExerciseById()
    {
        var exercise = _repository.GetExercise(3, 2);
        Assert.NotNull(exercise);
        Assert.Equal("Agrupar com GroupBy", exercise!.Title);
    }

    [Fact]
    public void GetWeek_UnknownId_ReturnsNull()
    {
        Assert.Null(_repository.GetWeek(999));
    }

    [Fact]
    public void Lessons_KeepOrderField()
    {
        var week = _repository.GetWeek(1);
        Assert.Equal(new[] { 1, 2, 3 }, week!.Lessons.Select(l => l.Order).ToArray());
    }
}
