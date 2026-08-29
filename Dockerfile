# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copia o conteúdo da pasta PortalEstudos/ para /src (csproj fica em /src/PortalEstudos.csproj)
COPY PortalEstudos/ ./
# Restore usando cache NuGet (offline-friendly; no Railway usa a rede normalmente)
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore PortalEstudos.csproj
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish PortalEstudos.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "PortalEstudos.dll"]
