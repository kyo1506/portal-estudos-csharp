using PortalEstudos.Domain.Entities;

namespace PortalEstudos.Application.Abstractions;

public record AuthResult(bool Success, Usuario? User, string? ErrorMessage);

public interface IAuthService
{
    Task<AuthResult> AuthenticateAsync(string email, string password, CancellationToken ct = default);
    Task<AuthResult> RegisterAsync(string nome, string email, string password, string role = Roles.Aluno, CancellationToken ct = default);
    Task<IReadOnlyList<Usuario>> ListUsersAsync(CancellationToken ct = default);
}
