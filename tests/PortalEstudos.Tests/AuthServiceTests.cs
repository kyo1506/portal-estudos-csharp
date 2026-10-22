using PortalEstudos.Application.Abstractions;
using PortalEstudos.Application.Services;
using PortalEstudos.Domain.Entities;
using Xunit;

namespace PortalEstudos.Tests;

public class AuthServiceTests
{
    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly List<Usuario> _users = new();

        public void Add(Usuario user) => _users.Add(user);

        public Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            var normalized = email.Trim().ToLowerInvariant();
            return Task.FromResult(_users.FirstOrDefault(u => u.Email.ToLower() == normalized));
        }

        public Task<Usuario?> GetByIdAsync(int id, CancellationToken ct = default)
            => Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

        public Task<Usuario> CreateAsync(Usuario usuario, CancellationToken ct = default)
        {
            usuario.Id = _users.Count + 1;
            _users.Add(usuario);
            return Task.FromResult(usuario);
        }

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        {
            var normalized = email.Trim().ToLowerInvariant();
            return Task.FromResult(_users.Any(u => u.Email.ToLower() == normalized));
        }

        public Task<IReadOnlyList<Usuario>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<Usuario>>(_users.ToList());
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password) => $"hash:{password}";
        public bool VerifyPassword(string password, string hash) => hash == $"hash:{password}";
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ReturnsSuccessAndUser()
    {
        var repo = new FakeUserRepository();
        var hasher = new FakePasswordHasher();
        repo.Add(new Usuario
        {
            Id = 1,
            Nome = "Aluno Teste",
            Email = "aluno@portal.com",
            SenhaHash = hasher.HashPassword("Portal@123"),
            Role = Roles.Aluno
        });

        var service = new AuthService(repo, hasher);

        var result = await service.AuthenticateAsync("ALUNO@portal.com", "Portal@123");

        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.Equal("Aluno Teste", result.User.Nome);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidPassword_ReturnsFailure()
    {
        var repo = new FakeUserRepository();
        var hasher = new FakePasswordHasher();
        repo.Add(new Usuario
        {
            Id = 1,
            Nome = "Instrutor",
            Email = "instrutor@portal.com",
            SenhaHash = hasher.HashPassword("Portal@123"),
            Role = Roles.Instrutor
        });

        var service = new AuthService(repo, hasher);

        var result = await service.AuthenticateAsync("instrutor@portal.com", "SenhaErrada");

        Assert.False(result.Success);
        Assert.Null(result.User);
        Assert.Equal("Credenciais inválidas.", result.ErrorMessage);
    }

    [Fact]
    public async Task AuthenticateAsync_WithUnknownUser_ReturnsFailure()
    {
        var repo = new FakeUserRepository();
        var hasher = new FakePasswordHasher();
        var service = new AuthService(repo, hasher);

        var result = await service.AuthenticateAsync("inexistente@portal.com", "QualquerSenha");

        Assert.False(result.Success);
        Assert.Null(result.User);
        Assert.Equal("Credenciais inválidas.", result.ErrorMessage);
    }

    [Theory]
    [InlineData("", "senha")]
    [InlineData("email@teste.com", "")]
    [InlineData("   ", "   ")]
    public async Task AuthenticateAsync_WithEmptyCredentials_ReturnsFailure(string email, string password)
    {
        var repo = new FakeUserRepository();
        var hasher = new FakePasswordHasher();
        var service = new AuthService(repo, hasher);

        var result = await service.AuthenticateAsync(email, password);

        Assert.False(result.Success);
        Assert.Null(result.User);
        Assert.Equal("Email e senha são obrigatórios.", result.ErrorMessage);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_CreatesUserAndReturnsSuccess()
    {
        var repo = new FakeUserRepository();
        var hasher = new FakePasswordHasher();
        var service = new AuthService(repo, hasher);

        var result = await service.RegisterAsync("Novo Aluno", "novo@portal.com", "senha123");

        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.Equal("Novo Aluno", result.User.Nome);
        Assert.Equal("novo@portal.com", result.User.Email);
        Assert.Equal(Roles.Aluno, result.User.Role);
        Assert.Equal("hash:senha123", result.User.SenhaHash);
    }

    [Fact]
    public async Task RegisterAsync_WithShortPassword_ReturnsError()
    {
        var repo = new FakeUserRepository();
        var hasher = new FakePasswordHasher();
        var service = new AuthService(repo, hasher);

        var result = await service.RegisterAsync("Aluno", "aluno@teste.com", "123");

        Assert.False(result.Success);
        Assert.Null(result.User);
        Assert.Contains("mínimo 6 caracteres", result.ErrorMessage);
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ReturnsError()
    {
        var repo = new FakeUserRepository();
        var hasher = new FakePasswordHasher();
        repo.Add(new Usuario
        {
            Nome = "Existente",
            Email = "duplicado@portal.com",
            SenhaHash = "hash:123456",
            Role = Roles.Aluno
        });

        var service = new AuthService(repo, hasher);
        var result = await service.RegisterAsync("Outro Nome", "DUPLICADO@portal.com", "senha123");

        Assert.False(result.Success);
        Assert.Contains("Já existe uma conta cadastrada", result.ErrorMessage);
    }

    [Fact]
    public async Task RegisterAsync_WithAdminRole_CreatesAdminSuccessfully()
    {
        var repo = new FakeUserRepository();
        var hasher = new FakePasswordHasher();
        var service = new AuthService(repo, hasher);

        var result = await service.RegisterAsync("Admin Sistema", "admin@portal.com", "segredo123", Roles.Admin);

        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.Equal(Roles.Admin, result.User.Role);
    }

    [Fact]
    public async Task ListUsersAsync_ReturnsAllUsers()
    {
        var repo = new FakeUserRepository();
        var hasher = new FakePasswordHasher();
        repo.Add(new Usuario { Nome = "User 1", Email = "u1@portal.com", Role = Roles.Aluno });
        repo.Add(new Usuario { Nome = "User 2", Email = "u2@portal.com", Role = Roles.Instrutor });

        var service = new AuthService(repo, hasher);
        var users = await service.ListUsersAsync();

        Assert.Equal(2, users.Count);
    }
}
