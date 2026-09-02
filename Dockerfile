# syntax=docker/dockerfile:1

# ---- Estágio de build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1) Copia APENAS os arquivos .csproj/.slnx/.props primeiro: qualquer mudança em código
#    não invalida o layer do `dotnet restore` (cache do Docker/Railway aproveitado).
#    Directory.Build.props é obrigatório: contém TargetFramework/Nullable/ImplicitUsings
#    compartilhados (sem ele o restore falha com NETSDK1013).
COPY *.slnx ./
COPY Directory.Build.props ./
COPY src/PortalEstudos.Web/PortalEstudos.Web.csproj src/PortalEstudos.Web/
COPY src/PortalEstudos.Application/PortalEstudos.Application.csproj src/PortalEstudos.Application/
COPY src/PortalEstudos.Domain/PortalEstudos.Domain.csproj src/PortalEstudos.Domain/
COPY src/PortalEstudos.Infrastructure/PortalEstudos.Infrastructure.csproj src/PortalEstudos.Infrastructure/
RUN dotnet restore src/PortalEstudos.Web/PortalEstudos.Web.csproj

# 2) Agora copia todo o código-fonte e publica. O restore incremental roda de novo
#    (cache NuGet quente, ~1s) porque o obj gerado no passo 1 é "seco" — sem wwwroot/
#    código ele não descobre os static web assets do shared framework (blazor.web.js),
#    e publish com --no-restore os deixaria de fora (404 em produção).
COPY src/ ./src/
RUN dotnet publish src/PortalEstudos.Web/PortalEstudos.Web.csproj -c Release -o /app/publish

# ---- Estágio runtime ----
# aspnet:10.0 (Ubuntu, com shell): inclui ca-certificates (HTTPS p/ api.github.com)
# e ICU (cultura pt-BR). Variantes chiseled cortam ~150MB mas exigem -extra p/ certs — não vale o risco sem teste local.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
# A porta é lida do env PORT pelo Program.cs (app.Run http://0.0.0.0:{port});
# este ENV é redundante e gerava warning de variável indefinida no docker build.
EXPOSE 8080
ENTRYPOINT ["dotnet", "PortalEstudos.Web.dll"]
