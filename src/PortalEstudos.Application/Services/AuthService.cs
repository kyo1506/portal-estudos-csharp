using PortalEstudos.Application.Abstractions;

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
}
