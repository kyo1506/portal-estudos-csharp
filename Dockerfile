# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY PortalEstudos/PortalEstudos.csproj PortalEstudos/
RUN dotnet restore PortalEstudos/PortalEstudos.csproj
COPY PortalEstudos/ ./
RUN dotnet publish PortalEstudos/PortalEstudos.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}
EXPOSE 8080
ENTRYPOINT ["dotnet", "PortalEstudos.dll"]
