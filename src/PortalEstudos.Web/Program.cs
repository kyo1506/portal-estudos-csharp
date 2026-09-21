using PortalEstudos.Web.Components;
using PortalEstudos.Application.Abstractions;
using PortalEstudos.Application.Services;
using PortalEstudos.Infrastructure.Content;
using PortalEstudos.Infrastructure.GitHub;
using PortalEstudos.Infrastructure.Markdown;
using PortalEstudos.Infrastructure.Progress;
using MudBlazor;
using MudBlazor.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.DataProtection;

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

// --- Camada de Infraestrutura (adaptadores) ---
builder.Services.AddSingleton<IContentRepository, JsonContentRepository>();
builder.Services.AddScoped<IProgressStore, LocalStorageProgressStore>();
builder.Services.AddScoped<IMarkdownRenderer, MarkdigMarkdownRenderer>();
builder.Services.AddHttpClient<IGitHubApi, GitHubApiClient>();

// --- Camada de Aplicação (casos de uso) ---
builder.Services.AddSingleton<ICatalogService, CatalogService>();
builder.Services.AddSingleton<IDashboardService, DashboardService>();
builder.Services.AddSingleton<IExerciseEvaluationService, ExerciseEvaluationService>();
builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<IChallengeStatusService, ChallengeStatusService>();

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
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Bind na porta configurada (PORT ou 8080 por padrão).
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");
