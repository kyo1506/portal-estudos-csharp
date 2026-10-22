using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using PortalEstudos.Application.Abstractions;

namespace PortalEstudos.Web.Services;

/// <summary>Recupera o identificador do usuário autenticado no contexto do Blazor.</summary>
public sealed class BlazorCurrentUserAccessor : ICurrentUserAccessor
{
    private readonly AuthenticationStateProvider _authStateProvider;

    public BlazorCurrentUserAccessor(AuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    public async Task<int?> GetCurrentUserIdAsync()
    {
        try
        {
            var state = await _authStateProvider.GetAuthenticationStateAsync();
            var user = state.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                var idStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idStr, out var id))
                {
                    return id;
                }
            }
        }
        catch
        {
            // Contexto estático (SSR) ou provider inacessível
        }

        return null;
    }
}
