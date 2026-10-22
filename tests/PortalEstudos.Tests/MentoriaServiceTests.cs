using PortalEstudos.Application.Abstractions;
using PortalEstudos.Application.Services;
using PortalEstudos.Domain.Entities;
using Xunit;

namespace PortalEstudos.Tests;

public class MentoriaServiceTests
{
    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly List<Usuario> _users = new();

        public void Add(Usuario user) => _users.Add(user);

        public Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default)
            => Task.FromResult(_users.FirstOrDefault(u => u.Email == email));

        public Task<Usuario?> GetByIdAsync(int id, CancellationToken ct = default)
            => Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

        public Task<Usuario> CreateAsync(Usuario usuario, CancellationToken ct = default)
        {
            _users.Add(usuario);
            return Task.FromResult(usuario);
        }

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
            => Task.FromResult(_users.Any(u => u.Email == email));

        public Task<IReadOnlyList<Usuario>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Usuario>>(_users.ToList());
    }

    private sealed class FakeUserProgressRepository : IUserProgressRepository
    {
        public Dictionary<int, UserProgress> Progresses { get; } = new();
        public List<SubmissaoExercicio> Submissions { get; } = new();

        public Task<UserProgress> GetProgressAsync(int usuarioId, CancellationToken ct = default)
        {
            if (!Progresses.TryGetValue(usuarioId, out var p))
            {
                p = new UserProgress();
                Progresses[usuarioId] = p;
            }
            return Task.FromResult(p);
        }

        public Task MarkLessonAsync(int usuarioId, int faseId, int lessonId, bool completed, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task RecordExerciseSubmissionAsync(int usuarioId, int faseId, int exerciseId, string code, bool isCorrect, string? errorMessage = null, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task MergeProgressAsync(int usuarioId, UserProgress localProgress, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task<IReadOnlyList<SubmissaoExercicio>> GetSubmissionsByUserAsync(int usuarioId, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<SubmissaoExercicio>>(Submissions.Where(s => s.UsuarioId == usuarioId).ToList());

        public Task<IReadOnlyList<SubmissaoExercicio>> GetAllSubmissionsAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<SubmissaoExercicio>>(Submissions.ToList());
    }

    [Fact]
    public async Task GetTurmaOverviewAsync_CalculatesAggregatedMetrics()
    {
        var userRepo = new FakeUserRepository();
        userRepo.Add(new Usuario { Id = 1, Nome = "Beatriz", Email = "beatriz@portal.com", Role = Roles.Aluno });
        userRepo.Add(new Usuario { Id = 2, Nome = "Carlos", Email = "carlos@portal.com", Role = Roles.Aluno });
        userRepo.Add(new Usuario { Id = 3, Nome = "Professor", Email = "prof@portal.com", Role = Roles.Instrutor });

        var progressRepo = new FakeUserProgressRepository();
        progressRepo.Progresses[1] = new UserProgress
        {
            CompletedLessons = { 1, 2, 3, 4 },
            CompletedExercises = { 10, 11 },
            LastActivity = DateTime.UtcNow
        };
        progressRepo.Progresses[2] = new UserProgress
        {
            CompletedLessons = { 1, 2 },
            CompletedExercises = { 10 },
            LastActivity = DateTime.UtcNow
        };

        var catalogRepo = new FakeContentRepository(TestData.BuildWeek(1, lessonCount: 4, exerciseCount: 2));
        var catalogService = new CatalogService(catalogRepo);

        var service = new MentoriaService(userRepo, progressRepo, catalogService);

        var overview = await service.GetTurmaOverviewAsync();

        Assert.Equal(2, overview.TotalAlunos); // ignora o instrutor
        Assert.Equal(3.0, overview.MediaLicoesConcluidas); // (4 + 2) / 2 = 3.0
        Assert.Equal(1.5, overview.MediaExerciciosConcluidos); // (2 + 1) / 2 = 1.5
        Assert.Equal(2, overview.Alunos.Count);
    }

    [Fact]
    public async Task GetTurmaOverviewAsync_FiltersAlertsWithThreeOrMoreAttempts()
    {
        var userRepo = new FakeUserRepository();
        userRepo.Add(new Usuario { Id = 1, Nome = "Aluno", Email = "aluno@portal.com", Role = Roles.Aluno });

        var progressRepo = new FakeUserProgressRepository();
        progressRepo.Submissions.Add(new SubmissaoExercicio
        {
            UsuarioId = 1,
            FaseId = 1,
            ExercicioId = 1,
            Tentativas = 1,
            PassouNosTestes = true
        });
        progressRepo.Submissions.Add(new SubmissaoExercicio
        {
            UsuarioId = 1,
            FaseId = 1,
            ExercicioId = 2,
            Tentativas = 3,
            PassouNosTestes = false,
            CodigoSubmetido = "int x = 0;",
            MensagemErro = "Erro de lógica"
        });

        var catalogRepo = new FakeContentRepository(TestData.BuildWeek(1, 2, 2));
        var catalogService = new CatalogService(catalogRepo);

        var service = new MentoriaService(userRepo, progressRepo, catalogService);

        var overview = await service.GetTurmaOverviewAsync();

        Assert.Equal(1, overview.TotalAlertasDificuldade);
        Assert.Single(overview.Alertas);
        Assert.Equal("Aluno", overview.Alertas[0].NomeAluno);
        Assert.Equal(3, overview.Alertas[0].Tentativas);
        Assert.Equal("int x = 0;", overview.Alertas[0].CodigoSubmetido);
    }

    [Fact]
    public async Task GetAlunoDetalhesAsync_ReturnsEnrichedSubmissionsAndCode()
    {
        var userRepo = new FakeUserRepository();
        userRepo.Add(new Usuario
        {
            Id = 5,
            Nome = "Beatriz",
            Email = "beatriz@portal.com",
            Role = Roles.Aluno,
            CriadoEm = new DateTime(2026, 1, 1)
        });

        var progressRepo = new FakeUserProgressRepository();
        progressRepo.Progresses[5] = new UserProgress
        {
            CompletedLessons = { 10 },
            CompletedExercises = { 20 }
        };
        progressRepo.Submissions.Add(new SubmissaoExercicio
        {
            UsuarioId = 5,
            FaseId = 1,
            ExercicioId = 20,
            CodigoSubmetido = "Console.WriteLine(\"resolvido\");",
            PassouNosTestes = true,
            Tentativas = 2,
            AtualizadoEm = DateTime.UtcNow
        });

        var catalogRepo = new FakeContentRepository(TestData.BuildWeek(1, 1, 1));
        var catalogService = new CatalogService(catalogRepo);

        var service = new MentoriaService(userRepo, progressRepo, catalogService);

        var detalhes = await service.GetAlunoDetalhesAsync(5);

        Assert.NotNull(detalhes);
        Assert.Equal("Beatriz", detalhes.Nome);
        Assert.Equal(1, detalhes.TotalLicoes);
        Assert.Equal(1, detalhes.TotalExercicios);
        Assert.Single(detalhes.Submissoes);
        Assert.Equal("Console.WriteLine(\"resolvido\");", detalhes.Submissoes[0].CodigoSubmetido);
        Assert.True(detalhes.Submissoes[0].PassouNosTestes);

        var inexistente = await service.GetAlunoDetalhesAsync(999);
        Assert.Null(inexistente);
    }
}
