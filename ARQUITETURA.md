# Arquitetura do Projeto de Estudos C#

O projeto está dividido em **dois repositórios** distintos:

## 1. Portal de Estudos (este repositório)
**URL:** https://github.com/kyo1506/portal-estudos-csharp

Aplicação web Blazor Server organizada em **Clean Architecture** (solução `PortalEstudos.slnx`).

### Estrutura em camadas

```
src/
├── PortalEstudos.Domain/          # Entidades + enums (sem dependências externas)
├── PortalEstudos.Application/     # Ports (interfaces) + casos de uso + DTOs
├── PortalEstudos.Infrastructure/  # Adaptadores de infraestrutura
└── PortalEstudos.Web/             # Blazor UI + composition root (Program.cs)
tests/
└── PortalEstudos.Tests/           # Testes unitários (xUnit)
```

Dependências (inversão de dependência): `Web → Application → Domain` e
`Infrastructure → Application → Domain`. Nenhuma camada interna conhece as externas;
o `Program.cs` da Web registra os adaptadores e serviços no contêiner de DI.

### Camadas

- **Domain** (`PortalEstudos.Domain`): entidades `Week`, `LessonModel`, `ExerciseModel`,
  `Challenge`, `UserProgress` e o enum `ExerciseDifficulty`. Sem referências a outras camadas.
- **Application** (`PortalEstudos.Application`): declara os *ports*
  `IContentRepository`, `IProgressStore`, `IGitHubApi`, `IMarkdownRenderer` e implementa os
  casos de uso `CatalogService` (consultas), `DashboardService` (estatísticas/progresso),
  `ProgressService` (operações de progresso, com cache em memória), `ExerciseEvaluationService`
  (verificação de código, lógica pura testável) e `ChallengeStatusService` (status de PR com cache).
- **Infrastructure** (`PortalEstudos.Infrastructure`): adaptadores
  `JsonContentRepository` (lê `Content/ContentSeed.json`, recurso embutido),
  `LocalStorageProgressStore` (persistência via `localStorage`),
  `GitHubApiClient` (API de PRs do GitHub) e `MarkdigMarkdownRenderer` (Markdown com cache).
- **Web** (`PortalEstudos.Web`): páginas `.razor` finas (apresentação), layout, tema e o
  composition root em `Program.cs` (DI, DataProtection, ResponseCompression, MudBlazor).

### Conteúdo do curso
O conteúdo das 4 semanas fica em `src/PortalEstudos.Infrastructure/Content/ContentSeed.json`
(recurso embutido), carregado por `JsonContentRepository`. Separar dados de comportamento
permite editar o conteúdo sem recompilar.

## 2. Repositório de Desafios (fundamentos-csharp)
**URL:** https://github.com/kyo1506/fundamentos-csharp

Contém os desafios semanais que exigem **Pull Request para code review**:
- `Semana-01/desafios-semanais/`
- `Semana-02/desafios-semanais/`
- `Semana-03/desafios-semanais/`
- `Semana-04/desafios-semanais/`

### Fluxo de Code Review
1. Aluna faz fork ou branch do repositório `fundamentos-csharp`
2. Resolve o desafio da semana
3. Abre um **Pull Request** para `main`
4. Mentor faz code review e aprova
5. Merge confirma a conclusão do desafio

## Como executar o Portal localmente

```bash
dotnet restore PortalEstudos.slnx
dotnet run --project src/PortalEstudos.Web/PortalEstudos.Web.csproj
```

Acesse: http://localhost:8080

## Testes

```bash
dotnet test PortalEstudos.slnx
```
