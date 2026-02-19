# 06 - Documentacao de Implantacao (DevOps)

Este guia descreve a implantacao tecnica do SIGAM com Docker, incluindo requisitos, variaveis de ambiente, banco de dados, operacao e escalabilidade.

## 1. Status da Implantacao

- Data de referencia: 19/02/2026.
- Status atual: implantado em ambiente de homologacao e em teste com usuarios.
- Janela de usuarios prevista: 15 a 20 usuarios ativos.
- Perfil de uso: endemico, com picos sazonais (ex.: periodo de consolidacao do PCA).

## 2. Pre-requisitos

- Docker Engine 24+.
- Docker Compose v2 (`docker compose`).
- Acesso de rede as portas:
`5173` (frontend), `5000` (backend API), `5432` (PostgreSQL), `8088` (servidor de imagens).

Validacao rapida:

```bash
docker --version
docker compose version
```

## 3. Variaveis de Ambiente (.env)

1. Copie o modelo:

```bash
cp .env.example .env
```

2. Edite o arquivo `.env` conforme o ambiente.

### 3.1 Modelo `.env.example` com significado das chaves

| Chave | Obrigatoria | Descricao |
|---|---|---|
| `DB_USER` | Sim | Usuario do PostgreSQL usado pela aplicacao. |
| `DB_PASSWORD` | Sim | Senha do PostgreSQL. |
| `DB_NAME` | Sim | Nome do banco principal da aplicacao. |
| `JWT_KEY` | Sim | Segredo usado para assinar tokens JWT (minimo 32 caracteres). |
| `JWT_ISSUER` | Sim | Valor esperado como emissor do JWT no backend. |
| `JWT_AUDIENCE` | Sim | Valor esperado como audiencia do JWT no backend. |
| `ASPNETCORE_ENVIRONMENT` | Sim | Ambiente de execucao do backend (`Development`, `Staging`, `Production`). |
| `SERVER_HOST` | Sim (producao) | Host/IP do servidor para CORS e URLs externas. Ex.: `10.0.0.10` ou dominio. |
| `RESEND_API_KEY` | Sim | Token da API de email (Resend). Sem essa chave o backend nao inicializa. |

Observacao:
- No Compose atual, a conexao de banco e montada automaticamente com `DB_USER`, `DB_PASSWORD` e `DB_NAME` via `ConnectionStrings__DefaultConnection`.
- Se sua politica DevOps exigir `DB_CONNECTION_STRING`, mantenha essa variavel em segredo no cofre de infraestrutura, mas o projeto versionado usa o padrao por chaves separadas.

## 4. Implantacao com Docker (Simples e Repetivel)

### 4.1 Ambiente de desenvolvimento/local

Subir todos os servicos:

```bash
docker compose up -d --build
```

Verificar status:

```bash
docker compose ps
docker compose logs -f backend-service
```

Parar ambiente:

```bash
docker compose down
```

Endpoints esperados:
- Frontend: `http://localhost:5173`
- Backend (Swagger): `http://localhost:5000/swagger`
- Imagens: `http://localhost:8088/images/`

### 4.2 Ambiente de producao

Este projeto possui `docker-compose.prod.yml` com imagens prontas:

```bash
docker compose -f docker-compose.prod.yml up -d
```

Recomenda-se:
- Definir `.env` com valores de producao antes do `up`.
- Fixar versoes de imagem (tag semantica) quando sair de homologacao para producao estavel.
- Colocar proxy reverso/TLS na frente dos servicos expostos.

## 5. Banco de Dados: Migrations e Seed

### 5.1 Comportamento padrao da aplicacao

Ao iniciar, o backend executa:
1. `Database.MigrateAsync()` (aplica migrations pendentes automaticamente).
2. Seed inicial de Centro, Departamentos e usuarios padrao, se tabelas estiverem vazias.

Ou seja, no fluxo normal **nao** e necessario rodar migracao manual.

### 5.2 Quando rodar manualmente

Use em troubleshooting ou pipelines controlados:

```bash
docker compose run --rm --entrypoint sh backend-service
dotnet ef database update
```

Criar nova migration:

```bash
docker compose run --rm --entrypoint sh backend-service
dotnet ef migrations add NomeDaMigration
```

## 6. Escalabilidade e Operacao

### 6.1 Escala para 15-20 usuarios

- O perfil atual (15-20 usuarios) e suportado por um unico host com Docker Compose.
- O gargalo principal costuma ser I/O de banco durante picos sazonais de solicitacoes.

### 6.2 Pico sazonal (uso endemico)

- Monitorar CPU/RAM de `backend-service` e `postgres-db`.
- Monitorar latencia e taxa de erro nas rotas de solicitacao e relatorios.
- Planejar janela de reforco de recursos nos periodos de maior demanda (ciclo PCA).

### 6.3 Possibilidade de escalonamento automatico

Docker Compose nao oferece auto scaling horizontal nativo por metrica.

Caminhos recomendados para auto scaling:
- Migrar para Kubernetes (HPA por CPU/memoria e replicas do backend).
- Usar ECS/Fargate com Auto Scaling, mantendo PostgreSQL gerenciado.
- Adicionar cache e/ou fila para amortecer picos de escrita e leitura.

## 7. Checklist rapido de implantacao

1. Preencher `.env` com segredos reais.
2. Subir stack com `docker compose up -d --build` (ou arquivo de producao).
3. Confirmar containers saudaveis com `docker compose ps`.
4. Validar Swagger (`/swagger`) e login no frontend.
5. Confirmar migrations e seed nos logs do backend.
6. Registrar data/status da rodada de implantacao para rastreabilidade.
