# Arquitetura do Projeto de Estudos C#

Este projeto está dividido em **dois repositórios** distintos:

## 1. Portal de Estudos (este repositório)
**URL:** https://github.com/kyo1506/portal-estudos-csharp

Aplicação web Blazor Server que contém:
- Teoria das 4 semanas
- Exemplos práticos de código
- Exercícios com editor integrado (verificação básica no navegador)
- Dashboard de progresso
- Links para os desafios

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

## Convenções

### Portal (Blazor)
- Páginas em `Components/Pages/`
- Modelos com sufixo `Model` para evitar conflito com nomes de arquivos `.razor`
- Serviço `IContentService` fornece todo o conteúdo

### Desafios (GitHub)
- Branch protection em `main` (1 PR review obrigatório)
- CI/CD via GitHub Actions
- Código em C# console/.NET

## Como executar o Portal localmente

```bash
cd PortalEstudos
dotnet run
```

Acesse: http://localhost:5000
