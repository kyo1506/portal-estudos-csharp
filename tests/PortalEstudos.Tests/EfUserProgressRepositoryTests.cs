using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PortalEstudos.Domain.Entities;
using PortalEstudos.Infrastructure.Data;
using Xunit;

namespace PortalEstudos.Tests;

public class EfUserProgressRepositoryTests
{
    private async Task<(PortalEstudosDbContext Context, SqliteConnection Connection)> CreateInMemoryContextAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<PortalEstudosDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new PortalEstudosDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var user = new Usuario
        {
            Id = 1,
            Nome = "Aluno Teste",
            Email = "aluno@teste.com",
            SenhaHash = "hash",
            Role = Roles.Aluno
        };
        context.Usuarios.Add(user);
        await context.SaveChangesAsync();

        return (context, connection);
    }

    [Fact]
    public async Task MarkLessonAsync_AddsAndRemovesLessons()
    {
        var (context, connection) = await CreateInMemoryContextAsync();
        using (connection)
        using (context)
        {
            var repo = new EfUserProgressRepository(context);

            // Marca lição como concluída
            await repo.MarkLessonAsync(usuarioId: 1, faseId: 2, lessonId: 10, completed: true);

            var progress = await repo.GetProgressAsync(1);
            Assert.Contains(10, progress.CompletedLessons);

            // Desmarca lição
            await repo.MarkLessonAsync(usuarioId: 1, faseId: 2, lessonId: 10, completed: false);

            var updated = await repo.GetProgressAsync(1);
            Assert.DoesNotContain(10, updated.CompletedLessons);
        }
    }

    [Fact]
    public async Task RecordExerciseSubmissionAsync_TracksAttemptsAndStatus()
    {
        var (context, connection) = await CreateInMemoryContextAsync();
        using (connection)
        using (context)
        {
            var repo = new EfUserProgressRepository(context);

            // Primeira tentativa: errou
            await repo.RecordExerciseSubmissionAsync(
                usuarioId: 1,
                faseId: 1,
                exerciseId: 5,
                code: "int a = 0;",
                isCorrect: false,
                errorMessage: "Saída incorreta");

            var progress1 = await repo.GetProgressAsync(1);
            Assert.DoesNotContain(5, progress1.CompletedExercises);
            Assert.Equal("int a = 0;", progress1.ExerciseAnswers[5]);

            var subs1 = await repo.GetSubmissionsByUserAsync(1);
            Assert.Single(subs1);
            Assert.Equal(1, subs1[0].Tentativas);
            Assert.False(subs1[0].PassouNosTestes);

            // Segunda tentativa: acertou
            await repo.RecordExerciseSubmissionAsync(
                usuarioId: 1,
                faseId: 1,
                exerciseId: 5,
                code: "int a = 10;",
                isCorrect: true);

            var progress2 = await repo.GetProgressAsync(1);
            Assert.Contains(5, progress2.CompletedExercises);
            Assert.Equal("int a = 10;", progress2.ExerciseAnswers[5]);

            var subs2 = await repo.GetSubmissionsByUserAsync(1);
            Assert.Single(subs2);
            Assert.Equal(2, subs2[0].Tentativas);
            Assert.True(subs2[0].PassouNosTestes);
        }
    }

    [Fact]
    public async Task MergeProgressAsync_PromotesAnonymousProgressToUserAccount()
    {
        var (context, connection) = await CreateInMemoryContextAsync();
        using (connection)
        using (context)
        {
            var repo = new EfUserProgressRepository(context);

            var anonymousProgress = new UserProgress
            {
                CompletedLessons = { 1, 2, 3 },
                CompletedExercises = { 101 },
                ExerciseAnswers = { [101] = "Console.WriteLine(\"ok\");" }
            };

            await repo.MergeProgressAsync(usuarioId: 1, anonymousProgress);

            var progress = await repo.GetProgressAsync(1);
            Assert.Equal(3, progress.CompletedLessons.Count);
            Assert.Contains(101, progress.CompletedExercises);
            Assert.Equal("Console.WriteLine(\"ok\");", progress.ExerciseAnswers[101]);
        }
    }
}
