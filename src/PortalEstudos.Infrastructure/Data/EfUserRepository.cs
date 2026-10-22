using Microsoft.EntityFrameworkCore;
using PortalEstudos.Application.Abstractions;
using PortalEstudos.Domain.Entities;

namespace PortalEstudos.Infrastructure.Data;

public sealed class EfUserRepository : IUserRepository
{
    private readonly PortalEstudosDbContext _context;

    public EfUserRepository(PortalEstudosDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalized, ct);
    }

    public async Task<Usuario?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<Usuario> CreateAsync(Usuario usuario, CancellationToken ct = default)
    {
        usuario.Email = usuario.Email.Trim().ToLowerInvariant();
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync(ct);
        return usuario;
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await _context.Usuarios
            .AnyAsync(u => u.Email.ToLower() == normalized, ct);
    }

    public async Task<IReadOnlyList<Usuario>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Usuarios
            .AsNoTracking()
            .OrderBy(u => u.Nome)
            .ToListAsync(ct);
    }
}
