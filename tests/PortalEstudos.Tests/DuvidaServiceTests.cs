using PortalEstudos.Application.Abstractions;
using PortalEstudos.Application.Dtos;
using PortalEstudos.Application.Services;
using PortalEstudos.Domain.Entities;
using Xunit;

namespace PortalEstudos.Tests;

public class DuvidaServiceTests
{
    private sealed class FakeDuvidaRepository : IDuvidaRepository
    {
        private readonly List<DuvidaMentoria> _duvidas = new();

        public Task<DuvidaMentoria> CriarAsync(DuvidaMentoria duvida, CancellationToken ct = default)
        {
            duvida.Id = _duvidas.Count + 1;
            _duvidas.Add(duvida);
            return Task.FromResult(duvida);
        }

        public Task<DuvidaMentoria?> ObterPorIdAsync(int id, CancellationToken ct = default)
            => Task.FromResult(_duvidas.FirstOrDefault(d => d.Id == id));

        public Task<IReadOnlyList<DuvidaMentoria>> ListarPorUsuarioAsync(int usuarioId, CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<DuvidaMentoria>>(_duvidas.Where(d => d.UsuarioId == usuarioId).ToList());

        public Task<IReadOnlyList<DuvidaMentoria>> ListarTodasAsync(string? status = null, CancellationToken ct = default)
        {
            var q = _duvidas.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(status))
                q = q.Where(d => d.Status == status);

            return Task.FromResult<IReadOnlyList<DuvidaMentoria>>(q.ToList());
        }

        public Task AtualizarAsync(DuvidaMentoria duvida, CancellationToken ct = default)
        {
            var idx = _duvidas.FindIndex(d => d.Id == duvida.Id);
            if (idx >= 0) _duvidas[idx] = duvida;
            return Task.CompletedTask;
        }
    }

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

    [Fact]
    public async Task EnviarDuvidaAsync_CreatesPendingDuvidaWithContext()
    {
        var duvidaRepo = new FakeDuvidaRepository();
        var userRepo = new FakeUserRepository();
        userRepo.Add(new Usuario { Id = 10, Nome = "Beatriz", Email = "beatriz@portal.com", Role = Roles.Aluno });

        var catalogRepo = new FakeContentRepository(TestData.BuildWeek(1, 2, 2));
        var catalogService = new CatalogService(catalogRepo);

        var service = new DuvidaService(duvidaRepo, userRepo, catalogService);

        var input = new CriarDuvidaInput(
            FaseId: 1,
            LicaoId: 2,
            ExercicioId: null,
            Titulo: "Dúvida sobre variáveis",
            Descricao: "Não entendi a diferença entre tipos de valor e referência.",
            TrechoCodigo: "int x = 10;"
        );

        var resultado = await service.EnviarDuvidaAsync(10, input);

        Assert.Equal(1, resultado.Id);
        Assert.Equal("Dúvida sobre variáveis", resultado.Titulo);
        Assert.Equal(DuvidaStatus.Pendente, resultado.Status);
        Assert.Equal("Beatriz", resultado.NomeAluno);
        Assert.Equal("int x = 10;", resultado.TrechoCodigo);
        Assert.Null(resultado.RespostaInstrutor);
    }

    [Fact]
    public async Task ObterMinhasDuvidasAsync_FiltersByUserId()
    {
        var duvidaRepo = new FakeDuvidaRepository();
        var userRepo = new FakeUserRepository();
        userRepo.Add(new Usuario { Id = 1, Nome = "Aluno 1", Email = "a1@portal.com", Role = Roles.Aluno });
        userRepo.Add(new Usuario { Id = 2, Nome = "Aluno 2", Email = "a2@portal.com", Role = Roles.Aluno });

        var catalogRepo = new FakeContentRepository(TestData.BuildWeek(1, 1, 1));
        var catalogService = new CatalogService(catalogRepo);

        var service = new DuvidaService(duvidaRepo, userRepo, catalogService);

        await service.EnviarDuvidaAsync(1, new CriarDuvidaInput(1, 1, null, "Duvida 1", "Desc", null));
        await service.EnviarDuvidaAsync(2, new CriarDuvidaInput(1, 1, null, "Duvida 2", "Desc", null));

        var minhasDuvidas = await service.ObterMinhasDuvidasAsync(1);

        Assert.Single(minhasDuvidas);
        Assert.Equal("Duvida 1", minhasDuvidas[0].Titulo);
    }

    [Fact]
    public async Task ResponderDuvidaAsync_SetsInstructorResponseAndStatus()
    {
        var duvidaRepo = new FakeDuvidaRepository();
        var userRepo = new FakeUserRepository();
        userRepo.Add(new Usuario { Id = 1, Nome = "Aluno", Email = "aluno@portal.com", Role = Roles.Aluno });

        var catalogRepo = new FakeContentRepository(TestData.BuildWeek(1, 1, 1));
        var catalogService = new CatalogService(catalogRepo);

        var service = new DuvidaService(duvidaRepo, userRepo, catalogService);

        var duvida = await service.EnviarDuvidaAsync(1, new CriarDuvidaInput(1, 1, null, "Duvida", "Como funciona?", null));

        var sucesso = await service.ResponderDuvidaAsync(duvida.Id, "Funciona assim: a referência aponta para a memória Heap.");

        Assert.True(sucesso);

        var atualizada = await service.ObterDuvidaAsync(duvida.Id);
        Assert.NotNull(atualizada);
        Assert.Equal(DuvidaStatus.Respondida, atualizada.Status);
        Assert.Equal("Funciona assim: a referência aponta para a memória Heap.", atualizada.RespostaInstrutor);
        Assert.NotNull(atualizada.RespondidaEm);
    }

    [Fact]
    public async Task ObterFilaAtendimentoAsync_FiltersByStatusCorrectly()
    {
        var duvidaRepo = new FakeDuvidaRepository();
        var userRepo = new FakeUserRepository();
        userRepo.Add(new Usuario { Id = 1, Nome = "Aluno", Email = "aluno@portal.com", Role = Roles.Aluno });

        var catalogRepo = new FakeContentRepository(TestData.BuildWeek(1, 1, 1));
        var catalogService = new CatalogService(catalogRepo);

        var service = new DuvidaService(duvidaRepo, userRepo, catalogService);

        var d1 = await service.EnviarDuvidaAsync(1, new CriarDuvidaInput(1, 1, null, "Duvida 1", "Desc", null));
        var d2 = await service.EnviarDuvidaAsync(1, new CriarDuvidaInput(1, 1, null, "Duvida 2", "Desc", null));

        await service.ResponderDuvidaAsync(d1.Id, "Resposta 1");

        var pendentes = await service.ObterFilaAtendimentoAsync(DuvidaStatus.Pendente);
        var respondidas = await service.ObterFilaAtendimentoAsync(DuvidaStatus.Respondida);
        var todas = await service.ObterFilaAtendimentoAsync();

        Assert.Single(pendentes);
        Assert.Equal("Duvida 2", pendentes[0].Titulo);

        Assert.Single(respondidas);
        Assert.Equal("Duvida 1", respondidas[0].Titulo);

        Assert.Equal(2, todas.Count);
    }
}
