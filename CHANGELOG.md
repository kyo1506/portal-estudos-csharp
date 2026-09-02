# Changelog — Portal de Estudos C#

Consolidação da sessão de estabilização (2026-09-02): correções de produção,
performance, DX, acessibilidade e SEO. Tudo commitado em `main` e em produção
(Railway, deploy automático via push).

---

## 2026-09-02 — Estabilização geral

### 🔴 Correções de produção

- **`insertBefore / n.parentNode is null`** (Blazor Server, todos os layouts):
  causado por `@rendermode` aplicado em `<HeadOutlet>` (hydration mismatch no
  SignalR renderer). Movido para `<Routes @rendermode="InteractiveServer" />`
  — HeadOutlet ficou estático. (`c3555f6`)
- **Footer não respeitava o drawer** (mobile): footer era filho do
  `<MudMainContent>` e colidia com o drawer overlay. Reestruturado:
  `.mud-layout` vira flex-column com `min-height:100vh`, footer é irmão do
  drawer e recebe `padding-left` via seletor `:has()` quando o drawer
  persistente está aberto. Sticky-bottom validado em 7 breakpoints via CDP.
  (`e8a0386`, `102a60e`)
- **Drawer invisível no mobile**: comportamento `Responsive` do MudBlazor
  9.9 — drawer inicia `--initial` + `display:none`. Verificado via CDP em
  viewport 375px que abre/fecha com overlay corretamente após o fix do footer.
  (sem commit próprio — coberto por `102a60e`)
- **`NETSDK1013` no Railway**: `Directory.Build.props` (TargetFramework
  centralizado no DX5) não era copiado no layer de restore do Dockerfile.
  (`9456995`)
- **`blazor.web.js` 404 em produção**: o publish usava `--no-restore` sobre um
  restore "seco" (feito antes do `COPY src/`, sem wwwroot) → o MSBuild não
  descobria os static web assets do shared framework e o `_framework/` não era
  publicado. Fix: publish **sem** `--no-restore` (restore incremental ~1s com
  cache quente). Lição: smoke test do container DEVE checar
  `/_framework/blazor.web.js` — HTML SSR carrega mesmo sem o script.
  (`dd7ac55`)
- **Texto literal `[truncated]` nos cards** (Dashboard/Weeks): artefato de
  diff truncado gravado no fonte. Removido; varredura no repo confirma zero
  ocorrências. (`1bc9056`)

### ⚡ Performance

- **Brotli** habilitado antes de gzip (`Content-Encoding: br` verificado);
  gzip como fallback. (`473ca55`)
- **Token GitHub opcional** (`GitHub:Token`): eleva rate limit da API de 60
  para 5000 req/h quando configurado; sem token, comportamento anônimo.
  (`473ca55`)

### 🛠️ Developer experience (DX)

- **Dockerfile com cache de camadas**: copia só `.csproj`/`.slnx`/`.props`
  antes do restore; código depois com publish. Builds incrementais reutilizam
  o restore. (`473ca55`, refinado em `9456995`, `dd7ac55`)
- **`global.json`**: SDK 10.0.400 fixo (evita drift de SDK). (`473ca55`)
- **CI (GitHub Actions)**: `build-test.yml` — build Release + 33 testes xUnit
  em todo push/PR para `main`. (`473ca55`)
- **`.editorconfig`**: utf-8/LF, indentação 4 (C#) / 2 (razor/html/json),
  convenções dotnet format. (`3556469`)
- **`Directory.Build.props`**: centraliza TargetFramework/Nullable/
  ImplicitUsings dos 5 projetos; csproj mantêm só o específico. (`a70aa19`)

### ♿ Acessibilidade

- **Cards clicáveis** (Dashboard/Weeks): `role="button"`, `tabindex="0"`,
  `aria-label` descritivo e navegação por **Enter/Space** (validado via CDP).
  (`a70aa19`)
- **aria-labels** no botão do menu e no toggle de tema. (`a70aa19`)

### 🔍 UX / SEO

- **Página 404 customizada**: `UseStatusCodePagesWithReExecute("/not-found")`
  + página `@page "/not-found"` + `NotFoundPage` no Router — cobre URL direta
  (HTTP 404 com corpo renderizado) e navegação SPA. `noindex` na página.
  (`473ca55`)
- **Skeleton de loading** em `/weeks` (única página sem loading state —
  eliminava o flash de "0 de 0 lições"). (`3556469`)
- **Meta description** em todas as páginas via `<HeadContent>` (SSR).
  (`a70aa19`)

---

## Guia de operação (armadilhas conhecidas)

- **Docker build local é obrigatório** antes de push quando o commit mexe em
  build config (csproj/props/slnx/Dockerfile): o CI do GitHub Actions roda só
  `dotnet build/test`, não valida a imagem.
- **Smoke test do container**: checar `/`, `/weeks`, `/nao-existe` (404 com
  corpo) **e `/_framework/blazor.web.js`** (200).
- **`dotnet publish` NÃO pode usar `--no-restore`** neste Dockerfile (ver
  `dd7ac55`).
- O `.dockerignore` exclui `*.md` — mudanças de README/CHANGELOG não
  invalidam o cache do build.
- `.hermes/` (planos locais) está no `.gitignore` — não commitar.
