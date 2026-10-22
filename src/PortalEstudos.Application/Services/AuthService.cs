using PortalEstudos.Application.Abstractions;
using PortalEstudos.Domain.Entities;

namespace PortalEstudos.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResult> AuthenticateAsync(string email, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return new AuthResult(false, null, "Email e senha são obrigatórios.");
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, ct);
        if (user == null)
        {
            return new AuthResult(false, null, "Credenciais inválidas.");
        }

        var isValid = _passwordHasher.VerifyPassword(password, user.SenhaHash);
        if (!isValid)
        {
            return new AuthResult(false, null, "Credenciais inválidas.");
        }

        return new AuthResult(true, user, null);
    }

    public async Task<AuthResult> RegisterAsync(string nome, string email, string password, string role = Roles.Aluno, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            return new AuthResult(false, null, "Nome é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            return new AuthResult(false, null, "Email informado é inválido.");
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            return new AuthResult(false, null, "A senha deve conter no mínimo 6 caracteres.");
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var jaExiste = await _userRepository.ExistsByEmailAsync(normalizedEmail, ct);
        if (jaExiste)
        {
            return new AuthResult(false, null, "Já existe uma conta cadastrada com este email.");
        }

        var roleFinal = role is Roles.Instrutor or Roles.Admin ? role : Roles.Aluno;
        var novoUsuario = new Usuario
        {
            Nome = nome.Trim(),
            Email = normalizedEmail,
            SenhaHash = _passwordHasher.HashPassword(password),
            Role = roleFinal,
            CriadoEm = DateTime.UtcNow
        };

        var salvo = await _userRepository.CreateAsync(novoUsuario, ct);
        return new AuthResult(true, salvo, null);
    }

    public Task<IReadOnlyList<Usuario>> ListUsersAsync(CancellationToken ct = default)
    {
        return _userRepository.GetAllAsync(ct);
    }
}
