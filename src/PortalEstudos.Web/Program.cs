using PortalEstudos.Web.Components;
using PortalEstudos.Web.Services;
using PortalEstudos.Application.Abstractions;
using PortalEstudos.Application.Services;
using PortalEstudos.Infrastructure.Content;
using PortalEstudos.Infrastructure.Data;
using PortalEstudos.Infrastructure.GitHub;
using PortalEstudos.Infrastructure.Markdown;
using PortalEstudos.Infrastructure.Progress;
using PortalEstudos.Infrastructure.Security;
using MudBlazor;
using MudBlazor.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Compressão de resposta: reduz o payload do Blazor Server (SignalR) e assets estáticos.
// Brotli primeiro (mais eficiente que gzip em texto/JS/CSS); o browser negocia via Accept-Encoding.
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream", "text/css", "application/javascript" });
});

// Estabiliza o circuito SignalR em hospedagens efêmeras (Render, Podman/Docker, etc.).
// As chaves de DataProtection PRECISAM sobreviver a redeploys se configuradas com volume persistente;
// caso contrário cada novo container gera chave nova e o cookie antiforgery do navegador não é descriptografado
// -> "antiforgery token could not be decrypted" -> circuito não sobe -> nada navega.
// Resolução em ordem de prioridade:
//   1. DATAPROTECTION_KEYS_PATH (configurado manualmente por env var)
//   2. /tmp/portal-estudos-keys (fallback padrão efêmero)
var keysPath = Environment.GetEnvironmentVariable("DATAPROTECTION_KEYS_PATH");
if (string.IsNullOrWhiteSpace(keysPath))
{
    keysPath = "/tmp/portal-estudos-keys";
}
var keysDir = new DirectoryInfo(keysPath);
keysDir.Create();
Console.WriteLine($"[DataProtection] Persistindo chaves em: {keysDir.FullName}");
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(keysDir)
    .SetApplicationName("portal-estudos-keys");

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();
builder.Services.AddMemoryCache();

// --- Persistência de Dados (EF Core Híbrido: SQLite / PostgreSQL) ---
builder.Services.AddPortalDbContext(builder.Configuration);

// --- Segurança e Autenticação RBAC ---
builder.Services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "PortalEstudos.Auth";
        options.LoginPath = "/login";
        options.LogoutPath = "/api/auth/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

// --- Camada de Infraestrutura (adaptadores) ---
builder.Services.AddSingleton<IContentRepository, JsonContentRepository>();
builder.Services.AddScoped<IProgressStore, LocalStorageProgressStore>();
builder.Services.AddScoped<IMarkdownRenderer, MarkdigMarkdownRenderer>();
builder.Services.AddHttpClient<IGitHubApi, GitHubApiClient>();

// --- Camada de Aplicação (casos de uso) ---
builder.Services.AddSingleton<ICatalogService, CatalogService>();
builder.Services.AddSingleton<IDashboardService, DashboardService>();
builder.Services.AddSingleton<IExerciseEvaluationService, ExerciseEvaluationService>();
builder.Services.AddScoped<ICurrentUserAccessor, BlazorCurrentUserAccessor>();
builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<IChallengeStatusService, ChallengeStatusService>();
builder.Services.AddScoped<IMentoriaService, MentoriaService>();
builder.Services.AddScoped<IDuvidaService, DuvidaService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // HSTS mantido desligado atrás de proxies de borda com terminação TLS (Render/Cloudflare) para evitar loops de redirecionamento.
}

// URLs desconhecidas (HTTP direto) caem aqui: re-executa para a rota /not-found,
// que renderiza a página 404 customizada mantendo o status HTTP 404.
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseResponseCompression();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Endpoints HTTP nativos para Login e Logout (compatíveis com emissão de cookies de sessão)
app.MapPost("/api/auth/login", async (HttpContext httpContext, IAuthService authService) =>
{
    var form = await httpContext.Request.ReadFormAsync();
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    var returnUrl = form["returnUrl"].ToString();

    var result = await authService.AuthenticateAsync(email, password);
    if (!result.Success || result.User == null)
    {
        var target = string.IsNullOrWhiteSpace(returnUrl)
            ? "/login?error=credenciais_invalidas"
            : $"/login?error=credenciais_invalidas&returnUrl={Uri.EscapeDataString(returnUrl)}";
        return Results.Redirect(target);
    }

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, result.User.Id.ToString()),
        new(ClaimTypes.Name, result.User.Nome),
        new(ClaimTypes.Email, result.User.Email),
        new(ClaimTypes.Role, result.User.Role)
    };

    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var authProperties = new AuthenticationProperties
    {
        IsPersistent = true,
        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
    };

    await httpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(claimsIdentity),
        authProperties);

    if (!string.IsNullOrWhiteSpace(returnUrl) &&
        !returnUrl.StartsWith("//") &&
        (returnUrl.StartsWith('/') || returnUrl.StartsWith('~')))
    {
        return Results.Redirect(returnUrl);
    }

    return Results.Redirect("/dashboard");
}).DisableAntiforgery();

app.MapPost("/api/auth/register", async (HttpContext httpContext, IAuthService authService) =>
{
    var form = await httpContext.Request.ReadFormAsync();
    var nome = form["nome"].ToString();
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    var confirmPassword = form["confirmPassword"].ToString();
    var returnUrl = form["returnUrl"].ToString();

    if (password != confirmPassword)
    {
        var target = string.IsNullOrWhiteSpace(returnUrl)
            ? "/login?tab=register&error=senhas_diferentes"
            : $"/login?tab=register&error=senhas_diferentes&returnUrl={Uri.EscapeDataString(returnUrl)}";
        return Results.Redirect(target);
    }

    var result = await authService.RegisterAsync(nome, email, password, PortalEstudos.Domain.Entities.Roles.Aluno);
    if (!result.Success || result.User == null)
    {
        var erro = Uri.EscapeDataString(result.ErrorMessage ?? "Falha ao cadastrar.");
        var target = string.IsNullOrWhiteSpace(returnUrl)
            ? $"/login?tab=register&error={erro}"
            : $"/login?tab=register&error={erro}&returnUrl={Uri.EscapeDataString(returnUrl)}";
        return Results.Redirect(target);
    }

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, result.User.Id.ToString()),
        new(ClaimTypes.Name, result.User.Nome),
        new(ClaimTypes.Email, result.User.Email),
        new(ClaimTypes.Role, result.User.Role)
    };

    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var authProperties = new AuthenticationProperties
    {
        IsPersistent = true,
        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
    };

    await httpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(claimsIdentity),
        authProperties);

    if (!string.IsNullOrWhiteSpace(returnUrl) &&
        !returnUrl.StartsWith("//") &&
        (returnUrl.StartsWith('/') || returnUrl.StartsWith('~')))
    {
        return Results.Redirect(returnUrl);
    }

    return Results.Redirect("/dashboard");
}).DisableAntiforgery();

app.MapGet("/api/auth/logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Inicializa o banco de dados e cria os usuários padrão de forma idempotente
await DatabaseConfiguration.MigrateAndSeedAsync(app.Services);

// Bind na porta configurada (PORT ou 8080 por padrão).
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");
