# 01 - Setup e Execução Local

Este guia cobre o setup e a operação local do ambiente de desenvolvimento.

Para implantação em homologação/produção, consulte `docs/06-IMPLANTACAO-DEVOPS.md`.
Para operações avançadas de banco de dados, consulte `docs/02-DATABASE.md`.

## Pré-requisitos

- Docker: [instalação oficial](https://docs.docker.com/get-docker/)
- Docker Compose v2 (`docker compose`)
- .NET SDK 8 (opcional para IntelliSense no VS Code)
- Node.js + NPM (opcional para IntelliSense no VS Code)

Validação:

```bash
docker --version
docker compose version
```

## Configuração Inicial

O projeto usa o arquivo `.env` na raiz.

1. Copie o modelo:

```bash
cp .env.example .env
```

2. Revise os valores conforme seu ambiente local.

## Subindo o Ambiente

```bash
docker compose up -d --build
```

## Endpoints Locais

- Frontend: `http://localhost:5173`
- Backend (Swagger): `http://localhost:5000/swagger`
- Servidor de imagens: `http://localhost:8088/images/`

## Comandos Úteis

| Comando | Descrição |
|---|---|
| `docker compose up -d --build` | Reconstrói imagens e sobe os serviços em background. |
| `docker compose up` | Sobe os serviços em primeiro plano com logs no terminal. |
| `docker compose ps` | Mostra o estado dos containers. |
| `docker compose logs -f` | Acompanha logs de todos os serviços. |
| `docker compose logs -f backend-service` | Acompanha logs apenas do backend. |
| `docker compose stop` | Para os containers sem removê-los. |
| `docker compose down` | Para e remove os containers (mantém o volume do banco). |
| `docker compose build --no-cache` | Reconstrói sem usar cache. |
| `docker compose exec backend-service dotnet test backend.csproj` | Executa testes .NET dentro do container do backend. |
| `docker compose exec frontend-service npm run test:unit` | Executa testes unitários do frontend (Vitest). |
| `docker compose exec cypress npx cypress run --e2e` | Executa testes E2E do Cypress no container dedicado. |
