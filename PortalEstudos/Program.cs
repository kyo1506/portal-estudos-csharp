using PortalEstudos.Components;
using PortalEstudos.Services;
using MudBlazor;
using MudBlazor.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.DataProtection;
using System.IO;

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
// Resolução em ordem de prioridade:
//   1. DATAPROTECTION_KEYS_PATH (configurado manualmente)
//   2. primeiro volume Railway montado em /var/lib/containers/railwayapp/bind-mounts/*/vol_* (auto-detectado)
//   3. /tmp/portal-estudos-keys (vida do container, só para dev local)
var keysPath = Environment.GetEnvironmentVariable("DATAPROTECTION_KEYS_PATH");
if (string.IsNullOrWhiteSpace(keysPath))
{
    var bindRoot = new DirectoryInfo("/var/lib/containers/railwayapp/bind-mounts");
    if (bindRoot.Exists)
    {
        var vol = bindRoot.EnumerateDirectories("vol_*", SearchOption.AllDirectories)
                           .OrderBy(d => d.FullName)
                           .FirstOrDefault();
        if (vol != null)
        {
            keysPath = Path.Combine(vol.FullName, "portal-estudos-keys");
        }
    }
}
keysPath ??= "/tmp/portal-estudos-keys";
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

