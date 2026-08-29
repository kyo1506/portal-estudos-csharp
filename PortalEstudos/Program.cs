using PortalEstudos.Components;
using PortalEstudos.Services;
using MudBlazor;
using MudBlazor.Services;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Compressão de resposta: reduz o payload do Blazor Server (SignalR) e assets no Railway free tier
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream", "text/css", "application/javascript" });
});

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
