# Guia de Contribuição

Este documento orienta novos desenvolvedores sobre como contribuir no projeto, executar testes e entender a arquitetura.

## 1. Antes de Começar

1. Leia `docs/01-SETUP.md`.
2. Copie o arquivo de ambiente:

```bash
cp .env.example .env
```

3. Suba os serviços:

```bash
docker compose up -d --build
```

Endpoints locais esperados:
- Frontend: `http://localhost:5173`
- Backend (Swagger): `http://localhost:5000/swagger`
- Servidor de imagens: `http://localhost:8088/images/`

## 2. Como Rodar Testes

Todos os comandos abaixo consideram o ambiente Docker em execução.

### 2.1 Backend (.NET)

Atualmente, o repositório não possui projeto de testes dedicado separado da API. Ainda assim, o comando padrão para testes .NET é:

```bash
docker compose exec backend-service dotnet test backend.csproj
```

Quando forem adicionados projetos de teste (por exemplo, em `backend/tests`), esse comando continuará sendo a referência principal.

### 2.2 Frontend Unitário (Vitest)

```bash
docker compose exec frontend-service npm run test:unit
```

Cobertura:

```bash
docker compose exec frontend-service npm run test:unit:coverage
```

### 2.3 Frontend E2E (Cypress)

Execução headless:

```bash
docker compose exec cypress npx cypress run --e2e
```

Modo interativo:

```bash
docker compose exec cypress npx cypress open --e2e
```

## 3. Arquitetura de Pastas (Visão Rápida)

### 3.1 Backend (`backend/src`)

- `Controllers/`: camada HTTP; recebe requisições e delega para serviços.
- `Services/`: regras de negócio e orquestração de casos de uso.
- `Services/Interfaces/`: contratos para injeção de dependência.
- `Models/Entities/`: entidades persistidas no banco.
- `Models/Dtos/`: contratos de entrada e saída da API.
- `Database/`: `AppDbContext` e seeders.
- `Migrations/` (em `backend/`): histórico de migrations do EF Core.

Fluxo típico:

`Controller -> Service -> AppDbContext/Entities -> DTO de resposta`

### 3.2 Frontend (`frontend/src`)

- `features/<dominio>/`: organização por domínio funcional.
- `features/*/views/`: telas.
- `features/*/components/`: componentes do domínio.
- `features/*/services/`: comunicação com API e adaptação de dados.
- `features/*/stores/`: estado compartilhado com Pinia.
- `features/*/types/`: contratos TypeScript do domínio.
- `features/*/utils/`: utilitários.
- `services/apiClient.ts`: cliente HTTP base.
- `router/`: rotas da aplicação.
- `stores/`: stores transversais (tema, layout, menu etc.).

Fluxo típico:

`View/Component -> Store (Pinia) -> Service -> API -> Store -> UI`

## 4. Conceitos-Chave

- `Controller` (backend): ponto de entrada HTTP (`[HttpGet]`, `[HttpPost]` etc.).
- `Service` (backend/frontend): encapsula regras e integrações.
- `Store` (Pinia): centraliza estado reativo e ações compartilhadas no frontend.

## 5. Como Contribuir no Dia a Dia

- Crie branch de feature/correção a partir da branch principal do time.
- Faça mudanças pequenas, objetivas e com escopo claro.
- Rode os testes relevantes antes de abrir PR.
- Atualize ou crie testes ao alterar comportamento.
- Descreva no PR: contexto, mudança, risco e como validar.

## 6. Sugestões para Evolução deste Documento

- Convenção de branches e padrão de commits.
- Checklist de PR (lint, testes, impacto em schema e capturas de tela de UI).
- Estratégia de versionamento e release.
- Guia de criação/aplicação de migrations com EF Core.
- Guia de debug (Docker, CORS, JWT, seed e banco).
- Regras de autorização por perfil (Solicitante, Gestor e Admin).
- Estratégia de dados de teste/fixtures para Cypress.
- Definição de pronto (Definition of Done).
