using PortalEstudos.Application.Services;
using PortalEstudos.Domain.Entities;
using Xunit;

namespace PortalEstudos.Tests;

public class DashboardServiceTests
{
    private readonly DashboardService _service;

    public DashboardServiceTests()
    {
        _service = new DashboardService(new FakeContentRepository(TestData.BuildWeek(1)));
    }

    [Fact]
    public void GetStats_EmptyProgress_ReturnsZeroAndTotals()
    {
        var stats = _service.GetStats(new UserProgress());

        Assert.Equal(1, stats.TotalFases);
        Assert.Equal(2, stats.TotalLessons);
        Assert.Equal(2, stats.TotalExercises);
        Assert.Equal(1, stats.TotalChallenges);
        Assert.Equal(0, stats.CompletedLessons);
        Assert.Equal(0, stats.CompletedExercises);
        Assert.Equal(0.0, stats.ProgressPercentage);
    }

    [Fact]
    public void GetStats_FullProgress_ReturnsOneHundredPercent()
    {
        var progress = new UserProgress
        {
            CompletedLessons = { 1, 2 },
            CompletedExercises = { 1, 2 },
            CompletedChallenges = { 1 }
        };

        var stats = _service.GetStats(progress);

        Assert.Equal(2, stats.CompletedLessons);
        Assert.Equal(2, stats.CompletedExercises);
        Assert.Equal(1, stats.CompletedChallenges);
        Assert.Equal(100.0, stats.ProgressPercentage);
    }

    [Fact]
    public void GetFaseProgress_PartialProgress_ReturnsRatio()
    {
        var progress = new UserProgress { CompletedLessons = { 1 } };
        var fase = TestData.BuildWeek(1);

        // 1 lição de um total de 5 itens (2 lições + 2 exercícios + 1 desafio).
        Assert.Equal(20.0, _service.GetFaseProgress(fase, progress));
    }

    [Fact]
    public void IsFaseCompleted_FullProgress_True()
    {
        var progress = new UserProgress
        {
            CompletedLessons = { 1, 2 },
            CompletedExercises = { 1, 2 },
            CompletedChallenges = { 1 }
        };
        Assert.True(_service.IsFaseCompleted(TestData.BuildWeek(1), progress));
    }

    [Fact]
    public void IsFaseCompleted_EmptyProgress_False()
    {
        Assert.False(_service.IsFaseCompleted(TestData.BuildWeek(1), new UserProgress()));
    }

    [Fact]
    public void IsFaseInProgress_Partial_True_None_False()
    {
        var partial = new UserProgress { CompletedExercises = { 1 } };
        Assert.True(_service.IsFaseInProgress(TestData.BuildWeek(1), partial));

        Assert.False(_service.IsFaseInProgress(TestData.BuildWeek(1), new UserProgress()));
    }

    [Fact]
    public void GetStats_WeekWithoutChallenge_TotalsCorrectly()
    {
        var service = new DashboardService(new FakeContentRepository(TestData.BuildWeek(1, hasChallenge: false)));
        var stats = service.GetStats(new UserProgress());

        Assert.Equal(0, stats.TotalChallenges);
        Assert.Equal(1, stats.TotalFases);
    }
}
