namespace PortalEstudos.Application.Abstractions;

public interface ICurrentUserAccessor
{
    Task<int?> GetCurrentUserIdAsync();
}
