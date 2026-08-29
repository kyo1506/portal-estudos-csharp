# Portal de Estudos C# e .NET

Portal web interativo para estudos de C# e fundamentos do .NET, construído com Blazor Server (.NET 8).

## 📚 Funcionalidades

- **Dashboard** com progresso de estudos
- **Lições** com teoria e exemplos de código
- **Exercícios** com editor de código integrado
- **Desafios semanais** (submetidos via GitHub PR)
- **Navegação** por semanas e tópicos

## 🛠️ Tecnologias

- **Blazor Server** (.NET 8)
- **Bootstrap 5** + Bootstrap Icons
- **C# 12**

## 📁 Estrutura

```
PortalEstudos/
├── Components/
│   ├── Layout/       # Layout principal e menu
│   ├── Pages/        # Páginas do portal
│   ├── App.razor     # Configuração de rotas
│   └── Routes.razor  # Router
├── Models/           # Modelos de dados
├── Services/         # Serviço de conteúdo
└── wwwroot/          # Arquivos estáticos
```

## 🚀 Como executar

```bash
cd PortalEstudos
dotnet run
```

Acesse: http://localhost:5000

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
