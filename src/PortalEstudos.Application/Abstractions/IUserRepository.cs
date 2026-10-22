using PortalEstudos.Domain.Entities;

namespace PortalEstudos.Application.Abstractions;

public interface IUserRepository
{
    Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<Usuario?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Usuario> CreateAsync(Usuario usuario, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<Usuario>> GetAllAsync(CancellationToken ct = default);
}
