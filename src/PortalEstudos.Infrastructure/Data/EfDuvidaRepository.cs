using Microsoft.EntityFrameworkCore;
using PortalEstudos.Application.Abstractions;
using PortalEstudos.Domain.Entities;

namespace PortalEstudos.Infrastructure.Data;

public sealed class EfDuvidaRepository : IDuvidaRepository
{
    private readonly PortalEstudosDbContext _context;

    public EfDuvidaRepository(PortalEstudosDbContext context)
    {
        _context = context;
    }

    public async Task<DuvidaMentoria> CriarAsync(DuvidaMentoria duvida, CancellationToken ct = default)
    {
        _context.DuvidasMentoria.Add(duvida);
        await _context.SaveChangesAsync(ct);
        return duvida;
    }

    public async Task<DuvidaMentoria?> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.DuvidasMentoria
            .Include(d => d.Usuario)
            .FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task<IReadOnlyList<DuvidaMentoria>> ListarPorUsuarioAsync(int usuarioId, CancellationToken ct = default)
    {
        return await _context.DuvidasMentoria
            .AsNoTracking()
            .Where(d => d.UsuarioId == usuarioId)
            .OrderByDescending(d => d.CriadaEm)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<DuvidaMentoria>> ListarTodasAsync(string? status = null, CancellationToken ct = default)
    {
        var query = _context.DuvidasMentoria
            .AsNoTracking()
            .Include(d => d.Usuario)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(d => d.Status == status);
        }

        return await query
            .OrderByDescending(d => d.CriadaEm)
            .ToListAsync(ct);
    }

    public async Task AtualizarAsync(DuvidaMentoria duvida, CancellationToken ct = default)
    {
        _context.DuvidasMentoria.Update(duvida);
        await _context.SaveChangesAsync(ct);
    }
}
