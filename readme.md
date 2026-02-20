# Sistema de Gestão de Aquisições de Materiais (SIGAM)

![Badge .NET](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet)
![Badge Vue.js](https://img.shields.io/badge/Vue.js-3-4FC08D?logo=vue.js)
![Badge PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql)
![Badge Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker)

## Visão Geral

O SIGAM é um sistema web full-stack desenvolvido como TCC para apoiar o levantamento de demandas do Plano de Contratações Anual (PCA) em instituições de ensino, com foco no CCET/UFS.

A proposta é substituir fluxos manuais em planilhas por uma plataforma centralizada, com rastreabilidade, padronização e mais transparência no processo de solicitações.

## Objetivos do Projeto

- Centralizar solicitações de materiais em um único sistema.
- Organizar catálogo de itens e categorias.
- Suportar diferentes perfis de acesso (Solicitante, Gestor e Admin).
- Disponibilizar painéis e relatórios para apoio à tomada de decisão.

## Funcionalidades Principais

- Autenticação via JWT com controle por papéis (RBAC).
- Cadastro e gestão de solicitações (gerais e patrimoniais).
- Fluxo de análise, ajustes e mudança de status de solicitações.
- Gestão de catálogo e histórico de itens.
- Dashboard gerencial e relatórios.

## Stack Tecnológica

- Backend: .NET 8, ASP.NET Core e Entity Framework Core.
- Frontend: Vue 3 (Composition API), Pinia, PrimeVue e Vite.
- Banco de dados: PostgreSQL.
- Testes frontend: Vitest (unitários) e Cypress (E2E).
- Infraestrutura local: Docker e Docker Compose.

## Arquitetura (Resumo)

- `backend/`: API REST, regras de negócio, acesso a dados e migrations.
- `frontend/`: SPA com organização por domínio (`features`), stores Pinia e serviços HTTP.
- `docs/`: documentação funcional, técnica e operacional.
- `docker-compose.yml`: orquestração do ambiente local.

Para detalhes da arquitetura por pasta e fluxo entre camadas, consulte `CONTRIBUTING.md`.

## Começo Rápido

Pré-requisitos:
- Docker
- Docker Compose v2 (`docker compose`)

Configuração inicial:

```bash
cp .env.example .env
```

Subida do ambiente:

```bash
docker compose up -d --build
```

Acessos locais:
- Frontend: `http://localhost:5173`
- Backend (Swagger): `http://localhost:5000/swagger`
- Servidor de imagens: `http://localhost:8088/images/`

## Testes (Resumo)

Com o ambiente já em execução:

- Backend (.NET):

```bash
docker compose exec backend-service dotnet test backend.csproj
```

- Frontend unitários (Vitest):

```bash
docker compose exec frontend-service npm run test:unit
```

- Frontend E2E (Cypress):

```bash
docker compose exec cypress npx cypress run --e2e
```

Guia detalhado de contribuição, testes e arquitetura: `CONTRIBUTING.md`.

## Documentação

### Desenvolvimento e operação

- [Guia de Contribuição](./CONTRIBUTING.md)
- [01 - Setup e Execução Local](./docs/01-SETUP.md)
- [02 - Gerenciamento do Banco de Dados](./docs/02-DATABASE.md)
- [06 - Documentação de Implantação (DevOps)](./docs/06-IMPLANTACAO-DEVOPS.md)
- [Frontend README](./frontend/README.md)

### Uso do sistema

- [03 - Manual do Usuário](./docs/03-MANUAL-USUARIO.md)
- [04 - Manual do Usuário (Resumido por Perfil)](./docs/04-MANUAL-USUARIO-RESUMIDO.md)
- [05 - Checklist Operacional de Onboarding](./docs/05-CHECKLIST-ONBOARDING-USUARIO.md)

## Boas Práticas para PR

- Manter PRs pequenas e com escopo claro.
- Rodar testes relevantes antes de abrir PR.
- Descrever contexto, mudança, risco e passos de validação.
- Atualizar documentação quando houver mudança de fluxo, regra ou operação.

## Status do Projeto

Projeto em evolução contínua, com foco em melhorias funcionais, cobertura de testes e robustez operacional.

## Licença

Ainda não há um arquivo de licença definido no repositório.
