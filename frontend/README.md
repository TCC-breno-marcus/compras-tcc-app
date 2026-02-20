# Frontend - SIGAM

Este documento resume os comandos do frontend considerando o ambiente Docker do projeto.

## Pré-requisito

Suba os serviços na raiz do repositório:

```bash
docker compose up -d --build
```

## Comandos principais (via Docker)

### Desenvolvimento

```bash
docker compose exec frontend-service npm run dev
```

### Build de produção

```bash
docker compose exec frontend-service npm run build
```

### Testes unitários (Vitest)

```bash
docker compose exec frontend-service npm run test:unit
```

Cobertura:

```bash
docker compose exec frontend-service npm run test:unit:coverage
```

### Testes E2E (Cypress)

Headless:

```bash
docker compose exec cypress npx cypress run --e2e
```

Interativo:

```bash
docker compose exec cypress npx cypress open --e2e
```

### Lint

```bash
docker compose exec frontend-service npm run lint
```
