# Portal de Estudos C# e .NET

Portal web interativo para estudos de C# e fundamentos do .NET, construído com Blazor Server (.NET 10) e organizado em **Clean Architecture**.

## 🚀 Funcionalidades

- **Dashboard** com progresso de estudos
- **Lições** com teoria e exemplos de código (Markdown)
- **Exercícios** com editor de código integrado e verificação no navegador
- **Desafios semanais** (submetidos via Pull Request no GitHub)
- **Navegação** por semanas e tópicos, com progresso persistido no navegador

## 🛠️ Tecnologias

- **Blazor Server** (.NET 10, render mode InteractiveServer)
- **MudBlazor** 9.x (UI)
- **Markdig** (renderização de Markdown)
- **xUnit** (testes)

## 🏗️ Arquitetura (Clean Architecture)

```
portal-estudos-csharp/
├── PortalEstudos.slnx
├── src/
│   ├── PortalEstudos.Domain/          # Entidades e enums (sem dependências)
│   ├── PortalEstudos.Application/     # Ports (interfaces), casos de uso, DTOs
│   ├── PortalEstudos.Infrastructure/  # Adaptadores: JSON, localStorage, GitHub, Markdown
│   └── PortalEstudos.Web/             # Blazor UI + Program.cs (composition root)
└── tests/
    └── PortalEstudos.Tests/           # Testes unitários (xUnit)
```

Regras de dependência (via inversão de dependência):

```
Web → Application → Domain
Infrastructure → Application → Domain
```

- O **Domain** define as entidades (`Week`, `LessonModel`, `ExerciseModel`, `Challenge`, `UserProgress`) sem conhecer infraestrutura.
- A **Application** declara os *ports* (`IContentRepository`, `IProgressStore`, `IGitHubApi`, `IMarkdownRenderer`) e implementa os casos de uso (`CatalogService`, `DashboardService`, `ProgressService`, `ExerciseEvaluationService`, `ChallengeStatusService`).
- A **Infrastructure** implementa os adaptadores: conteúdo em **JSON embutido** (`Content/ContentSeed.json`), progresso em `localStorage`, status de PRs via API do GitHub (com cache em memória) e Markdown.
- A **Web** apenas apresenta: injeta os serviços no `Program.cs` e mantém as páginas finas (sem lógica de domínio/persistência).

### Conteúdo do curso

O conteúdo das 4 semanas (lições, exercícios, desafios) fica em
`src/PortalEstudos.Infrastructure/Content/ContentSeed.json`, carregado por
`JsonContentRepository` como **recurso embutido** (`EmbeddedResource`).
**Para alterar o conteúdo é preciso editar o JSON e recompilar** (dotnet build).

## 📁 Como executar

```bash
cd portal-estudos-csharp
dotnet restore PortalEstudos.slnx
dotnet build PortalEstudos.slnx
dotnet run --project src/PortalEstudos.Web/PortalEstudos.Web.csproj
```

Acesse: http://localhost:8080 (a porta usa a variável `PORT` ou `8080` por padrão).

## ✅ Testes

```bash
dotnet test PortalEstudos.slnx
```

## 🐳 Deploy (Docker/Railway)

O deploy é automático: todo push para `main` dispara o build no Railway
(imagem Docker multi-stage com cache de camadas). CI adicional via GitHub
Actions (`build-test.yml`) valida build Release + testes a cada push/PR.

```bash
# Build local (obrigatório validar antes de push que mexe em build config)
docker build -t portal-estudos .
docker run -p 8080:8080 -e PORT=8080 portal-estudos
```

Smoke test do container: checar `/`, `/weeks`, uma URL inexistente (404 com
corpo renderizado) **e `/_framework/blazor.web.js` (200)** — o HTML SSR
carrega mesmo sem o script Blazor, então o 404 do script só aparece se for
testado explicitamente.

Variáveis de ambiente opcionais:

| Variável | Efeito |
|----------|--------|
| `GitHub:Token` | Token GitHub (fine-grained) — eleva o rate limit da API de 60 para 5000 req/h no status de PRs |
| `DATAPROTECTION_KEYS_PATH` | Onde persistir as chaves de DataProtection (detecta volume Railway automaticamente) |

## 📅 Semanas de Estudo

| Semana | Tópico |
|--------|--------|
| 1 | Memória, Tipos e a Base de Tudo |
| 2 | Programação Orientada a Objetos |
| 3 | Coleções, LINQ e Manipulação de Dados |
| 4 | ASP.NET Core e Deploy |

## 🔗 Repositório de Desafios

Os desafios semanais são submetidos via Pull Request no repositório:
https://github.com/kyo1506/fundamentos-csharp

## 📝 Licença

MIT
