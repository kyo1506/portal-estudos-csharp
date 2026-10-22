using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PortalEstudos.Application.Abstractions;
using PortalEstudos.Domain.Entities;
using PortalEstudos.Infrastructure.Data;
using PortalEstudos.Infrastructure.Security;
using Xunit;

namespace PortalEstudos.Tests;

public class DatabaseConfigurationAndRepositoryTests
{
    [Fact]
    public void ConvertPostgreSqlUriToConnectionString_ParsesRenderUriCorrectly()
    {
        var renderUri = "postgres://portal_user:secret_pass123@dpg-abc12345.oregon-postgres.render.com:5432/portal_db";

        var connStr = DatabaseConfiguration.ConvertPostgreSqlUriToConnectionString(renderUri);

        Assert.Contains("Host=dpg-abc12345.oregon-postgres.render.com", connStr);
        Assert.Contains("Port=5432", connStr);
        Assert.Contains("Database=portal_db", connStr);
        Assert.Contains("Username=portal_user", connStr);
        Assert.Contains("Password=secret_pass123", connStr);
        Assert.Contains("SSL Mode=Require", connStr);
    }

    [Fact]
    public void ConvertPostgreSqlUriToConnectionString_LeavesStandardConnectionStringUntouched()
    {
        var standardConnStr = "Host=localhost;Port=5432;Database=test;Username=postgres;Password=pass";

        var result = DatabaseConfiguration.ConvertPostgreSqlUriToConnectionString(standardConnStr);

        Assert.Equal(standardConnStr, result);
    }

    [Fact]
    public async Task EfUserRepository_CrudOperations_WorkCorrectly()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<PortalEstudosDbContext>()
            .UseSqlite(connection)
            .Options;

        using var context = new PortalEstudosDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var repository = new EfUserRepository(context);

        var usuario = new Usuario
        {
            Nome = "Beatriz",
            Email = "beatriz@portal.com",
            SenhaHash = "hash123",
            Role = Roles.Aluno
        };

        var criado = await repository.CreateAsync(usuario);
        Assert.True(criado.Id > 0);

        var existe = await repository.ExistsByEmailAsync("BEATRIZ@portal.com");
        Assert.True(existe);

        var buscadoPorEmail = await repository.GetByEmailAsync("beatriz@portal.com");
        Assert.NotNull(buscadoPorEmail);
        Assert.Equal("Beatriz", buscadoPorEmail.Nome);

        var buscadoPorId = await repository.GetByIdAsync(criado.Id);
        Assert.NotNull(buscadoPorId);
        Assert.Equal("beatriz@portal.com", buscadoPorId.Email);

        var todos = await repository.GetAllAsync();
        Assert.Single(todos);
    }

    [Fact]
    public async Task MigrateAndSeedAsync_PopulatesInitialUsersIdempotently()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddDbContext<PortalEstudosDbContext>(opt => opt.UseSqlite(connection));
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        var provider = services.BuildServiceProvider();

        // Primeira execução: deve criar os 2 usuários padrão
        await DatabaseConfiguration.MigrateAndSeedAsync(provider);

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PortalEstudosDbContext>();
            var count = await db.Usuarios.CountAsync();
            Assert.Equal(2, count);

            var instrutor = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == "instrutor@portal.com");
            Assert.NotNull(instrutor);
            Assert.Equal(Roles.Instrutor, instrutor.Role);

            var aluno = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == "aluno@portal.com");
            Assert.NotNull(aluno);
            Assert.Equal(Roles.Aluno, aluno.Role);
        }

        // Segunda execução: deve ser idempotente e não duplicar registros
        await DatabaseConfiguration.MigrateAndSeedAsync(provider);

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PortalEstudosDbContext>();
            var count = await db.Usuarios.CountAsync();
            Assert.Equal(2, count);
        }
    }
}
