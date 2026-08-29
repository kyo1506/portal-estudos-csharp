using PortalEstudos.Components;
using PortalEstudos.Services;
using MudBlazor;
using MudBlazor.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Compressão de resposta: reduz o payload do Blazor Server (SignalR) e assets no Railway free tier
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream", "text/css", "application/javascript" });
});

// Estabiliza o circuito SignalR no Railway (hospedagem efêmera).
// As chaves de DataProtection PRECISAM sobreviver a redeploys; caso contrário cada novo
// container gera chave nova e o cookie antiforgery do navegador não é descriptografado
// -> "antiforgery token could not be decrypted" -> circuito não sobe -> nada navega.
// Prioridade: (1) volume persistente apontado por DATAPROTECTION_KEYS_PATH; (2) fallback /tmp (vida do container).
var keysPath = Environment.GetEnvironmentVariable("DATAPROTECTION_KEYS_PATH") ?? "/tmp/portal-estudos-keys";
var keysDir = new DirectoryInfo(keysPath);
keysDir.Create();
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(keysDir)
    .SetApplicationName("portal-estudos-csharp");

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register application services
builder.Services.AddSingleton<IContentService, ContentService>();
builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddHttpClient<IGitHubService, GitHubService>();
builder.Services.AddMudServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // HSTS kept off behind Railway's TLS-terminating edge proxy to avoid redirect loops.
}

app.UseResponseCompression();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Bind to Railway's PORT (falls back to 8080 locally / other hosts).
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");

