using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PortalEstudos.Application.Abstractions;
using PortalEstudos.Domain.Entities;

namespace PortalEstudos.Infrastructure.Data;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddPortalDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
                          ?? configuration.GetConnectionString("DefaultConnection");

        if (!string.IsNullOrWhiteSpace(databaseUrl) &&
            (databaseUrl.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
             databaseUrl.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) ||
             databaseUrl.Contains("Host=", StringComparison.OrdinalIgnoreCase)))
        {
            var connectionString = ConvertPostgreSqlUriToConnectionString(databaseUrl);
            Console.WriteLine("[Database] Configurando PostgreSQL para persistência.");
            services.AddDbContext<PortalEstudosDbContext>(options =>
                options.UseNpgsql(connectionString));
        }
        else
        {
            var sqlitePath = Environment.GetEnvironmentVariable("SQLITE_DB_PATH") ?? "PortalEstudos.db";
            Console.WriteLine($"[Database] Configurando SQLite local: {sqlitePath}");
            services.AddDbContext<PortalEstudosDbContext>(options =>
                options.UseSqlite($"Data Source={sqlitePath}"));
        }

        services.AddScoped<IUserRepository, EfUserRepository>();
        return services;
    }

    public static string ConvertPostgreSqlUriToConnectionString(string databaseUrl)
    {
        if (string.IsNullOrWhiteSpace(databaseUrl)) return databaseUrl;
        if (!databaseUrl.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !databaseUrl.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return databaseUrl;
        }

        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "";
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');

        return $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    }

    public static async Task MigrateAndSeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PortalEstudosDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await db.Database.EnsureCreatedAsync();

        if (!await db.Usuarios.AnyAsync())
        {
            var instrutor = new Usuario
            {
                Nome = "Instrutor",
                Email = "instrutor@portal.com",
                SenhaHash = hasher.HashPassword("Portal@123"),
                Role = Roles.Instrutor,
                CriadoEm = DateTime.UtcNow
            };

            var aluno = new Usuario
            {
                Nome = "Aluno Inicial",
                Email = "aluno@portal.com",
                SenhaHash = hasher.HashPassword("Portal@123"),
                Role = Roles.Aluno,
                CriadoEm = DateTime.UtcNow
            };

            db.Usuarios.AddRange(instrutor, aluno);
            await db.SaveChangesAsync();
            Console.WriteLine("[Database] Seed inicial de usuários criado com sucesso.");
        }
    }
}
