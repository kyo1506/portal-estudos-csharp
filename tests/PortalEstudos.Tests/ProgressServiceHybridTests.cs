using PortalEstudos.Application.Abstractions;
using PortalEstudos.Application.Services;
using PortalEstudos.Domain.Entities;
using Xunit;

namespace PortalEstudos.Tests;

public class ProgressServiceHybridTests
{
    private sealed class FakeCurrentUserAccessor : ICurrentUserAccessor
    {
        public int? CurrentUserId { get; set; }
        public Task<int?> GetCurrentUserIdAsync() => Task.FromResult(CurrentUserId);
    }

    private sealed class FakeUserProgressRepository : IUserProgressRepository
    {
        public UserProgress Progress { get; set; } = new();
        public List<SubmissaoExercicio> Submissions { get; } = new();
        public bool MergedCalled { get; private set; }

        public Task<UserProgress> GetProgressAsync(int usuarioId, CancellationToken ct = default)
            => Task.FromResult(Progress);

        public Task MarkLessonAsync(int usuarioId, int faseId, int lessonId, bool completed, CancellationToken ct = default)
        {
            if (completed)
                Progress.CompletedLessons.Add(lessonId);
            else
                Progress.CompletedLessons.Remove(lessonId);

            return Task.CompletedTask;
        }

        public Task RecordExerciseSubmissionAsync(int usuarioId, int faseId, int exerciseId, string code, bool isCorrect, string? errorMessage = null, CancellationToken ct = default)
        {
            if (isCorrect)
                Progress.CompletedExercises.Add(exerciseId);

            Progress.ExerciseAnswers[exerciseId] = code;

            var sub = Submissions.FirstOrDefault(s => s.ExercicioId == exerciseId);
            if (sub != null)
            {
                sub.Tentativas++;
                sub.CodigoSubmetido = code;
                sub.PassouNosTestes = isCorrect || sub.PassouNosTestes;
                sub.MensagemErro = errorMessage;
            }
            else
            {
                Submissions.Add(new SubmissaoExercicio
                {
                    UsuarioId = usuarioId,
                    FaseId = faseId,
                    ExercicioId = exerciseId,
                    CodigoSubmetido = code,
                    PassouNosTestes = isCorrect,
                    MensagemErro = errorMessage,
                    Tentativas = 1
                });
            }

            return Task.CompletedTask;
        }

        public Task MergeProgressAsync(int usuarioId, UserProgress localProgress, CancellationToken ct = default)
        {
            MergedCalled = true;
            foreach (var l in localProgress.CompletedLessons)
                Progress.CompletedLessons.Add(l);

            foreach (var e in localProgress.CompletedExercises)
                Progress.CompletedExercises.Add(e);

            foreach (var kvp in localProgress.ExerciseAnswers)
                Progress.ExerciseAnswers[kvp.Key] = kvp.Value;

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<SubmissaoExercicio>> GetSubmissionsByUserAsync(int usuarioId, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<SubmissaoExercicio>>(Submissions);

        public Task<IReadOnlyList<SubmissaoExercicio>> GetAllSubmissionsAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<SubmissaoExercicio>>(Submissions);
    }

    [Fact]
    public async Task LoadAsync_WhenUserIsAuthenticated_MergesLocalProgressIntoDb()
    {
        var localStore = new InMemoryProgressStore
        {
            Stored = new UserProgress
            {
                CompletedLessons = { 1 },
                CompletedExercises = { 10 },
                ExerciseAnswers = { [10] = "var x = 1;" }
            }
        };

        var userRepo = new FakeUserProgressRepository
        {
            Progress = new UserProgress { CompletedLessons = { 2 } }
        };

        var userAccessor = new FakeCurrentUserAccessor { CurrentUserId = 1 };

        var service = new ProgressService(localStore, userRepo, userAccessor);

        var result = await service.LoadAsync();

        Assert.True(userRepo.MergedCalled);
        Assert.Contains(1, result.CompletedLessons);
        Assert.Contains(2, result.CompletedLessons);
        Assert.Contains(10, result.CompletedExercises);
        Assert.Equal("var x = 1;", result.ExerciseAnswers[10]);
    }

    [Fact]
    public async Task MarkLessonComplete_WhenAuthenticated_SavesToDbAndStore()
    {
        var localStore = new InMemoryProgressStore();
        var userRepo = new FakeUserProgressRepository();
        var userAccessor = new FakeCurrentUserAccessor { CurrentUserId = 1 };

        var service = new ProgressService(localStore, userRepo, userAccessor);

        await service.MarkLessonCompleteAsync(lessonId: 5, faseId: 1);

        Assert.Contains(5, userRepo.Progress.CompletedLessons);
        Assert.Contains(5, localStore.Stored.CompletedLessons);
    }

    [Fact]
    public async Task RecordExerciseAttempt_WhenFailed_TracksInDbWithoutMarkingCompleted()
    {
        var localStore = new InMemoryProgressStore();
        var userRepo = new FakeUserProgressRepository();
        var userAccessor = new FakeCurrentUserAccessor { CurrentUserId = 1 };

        var service = new ProgressService(localStore, userRepo, userAccessor);

        await service.RecordExerciseAttemptAsync(
            exerciseId: 20,
            code: "int a = 0;",
            isCorrect: false,
            errorMessage: "Erro de asserção",
            faseId: 3);

        Assert.DoesNotContain(20, userRepo.Progress.CompletedExercises);
        Assert.Equal("int a = 0;", userRepo.Progress.ExerciseAnswers[20]);
        Assert.Single(userRepo.Submissions);
        Assert.False(userRepo.Submissions[0].PassouNosTestes);
    }

    [Fact]
    public async Task LoadAsync_WhenAnonymous_UsesOnlyStore()
    {
        var localStore = new InMemoryProgressStore
        {
            Stored = new UserProgress { CompletedLessons = { 99 } }
        };
        var userRepo = new FakeUserProgressRepository();
        var userAccessor = new FakeCurrentUserAccessor { CurrentUserId = null };

        var service = new ProgressService(localStore, userRepo, userAccessor);

        var result = await service.LoadAsync();

        Assert.Contains(99, result.CompletedLessons);
        Assert.False(userRepo.MergedCalled);
        Assert.Empty(userRepo.Progress.CompletedLessons);
    }
}
