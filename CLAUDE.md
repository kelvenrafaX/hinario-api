# CLAUDE.md

Este arquivo fornece orientações ao Claude Code (claude.ai/code) ao trabalhar com o código deste repositório.

## Comandos

```bash
# Compilar a solução
dotnet build Hinario.sln

# Executar a API (HTTP na porta 5252)
dotnet run --project src/Hinario.API --launch-profile http

# Executar a API (HTTPS nas portas 7243/5252)
dotnet run --project src/Hinario.API --launch-profile https

# Aplicar migrations do EF Core
dotnet ef database update --project src/Hinario.Infra --startup-project src/Hinario.API

# Adicionar uma nova migration
dotnet ef migrations add <NomeDaMigration> --project src/Hinario.Infra --startup-project src/Hinario.API
```

Não há projetos de teste nesta solução.

## Arquitetura

Clean Architecture com quatro camadas. Fluxo de dependência: `API → Application → Domain ← Infra`.

- **Hinario.Domain** — entidades (`Hino`), DTOs, interfaces e utilitários. Sem dependências externas. Todos os contratos (IHinoService, IHinoRepository, ICampinaGrandeMineracaoService) residem aqui.
- **Hinario.Application** — implementa as interfaces de serviço do domínio. Contém `HinoService` (lógica de negócio principal) e `CampinaGrandeMineracaoService` (web scraper usando HtmlAgilityPack).
- **Hinario.Infra** — EF Core 10 com Npgsql (PostgreSQL). `HinarioApiContext` e `HinoRepository`. Contém as migrations.
- **Hinario.API** — ASP.NET Core 10 Web API. Controller único: `HinoController`. Configuração de DI e CORS em `Program.cs`.

## Modelo de Domínio

`Hino` (tabela: `hinos`): entidade central única.
- `Identificador` — chave string única como `"C-1"` ou `"H-10"` (anulável). O prefixo define o tipo do hino: H=Hinos, C=Cânticos.
- `LetraIdx` — coluna `tsvector` do PostgreSQL gerada automaticamente pelo servidor para busca de texto completo no idioma português.

## Pesquisa

A busca de texto completo é implementada no nível do PostgreSQL via tsvector + índice GIN na coluna `LetraIdx`. Os resultados são ordenados por: correspondência exata da frase > todas as palavras presentes > contagem de palavras. O `TrechoPesquisaHelper` (em `Hinario.Domain/Utils/`) gera trechos de pré-visualização com os termos encontrados envoltos em tags `<b>`.

O `TextNormalizer` (em `Hinario.Domain/Utils/`) remove acentos e converte para minúsculas — usado na busca por `Identificador` (hífens também são removidos, então `"C-1"` e `"c1"` resolvem para o mesmo hino).

## Endpoints Principais da API

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/hino` | Todos os hinos |
| GET | `/api/hino/{id}` | Por ID do banco de dados |
| GET | `/api/hino/identificador/{identificador}` | Por identificador (ex.: `C-1`) |
| GET | `/api/hino/pesquisar?texto=...` | Busca de texto completo |
| POST | `/api/hino` | Criar |
| PUT | `/api/hino/{id}` | Atualizar |
| POST | `/api/hino/importar` | Importação em lote (formato JSON HolyRycs) |
| GET | `/api/hino/minerar/canticos/{numero}` | Raspar hino de site externo (1–100) |
| GET | `/api/hino/{tipo}/{numero}/proximo` | Próximo hino na sequência do tipo |
| GET | `/api/hino/{tipo}/{numero}/anterior` | Hino anterior na sequência do tipo |

## Banco de Dados

PostgreSQL hospedado no Aiven. A string de conexão está em `appsettings.json` / `appsettings.Development.json`. O query splitting do EF Core está habilitado globalmente no DbContext. A coluna `Identificador` possui um índice único parcial (permitindo múltiplos NULLs).

## Web Scraper

O `CampinaGrandeMineracaoService` raspa hinos do site externo da Igreja em Campina Grande. Trata múltiplos encodings de caracteres (UTF-8, UTF-16, ISO-8859-1) e faz o parse do HTML com HtmlAgilityPack. Acessado via endpoint `/api/hino/minerar/canticos/{numero}`.
