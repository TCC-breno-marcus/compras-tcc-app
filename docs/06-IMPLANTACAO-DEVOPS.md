# 06 - Documentação de Implantação (DevOps)

Este guia descreve a implantação técnica do SIGAM com Docker, incluindo requisitos, variáveis de ambiente, banco de dados, operação e escalabilidade.

## 1. Status da Implantação

- Data de referência: 19/02/2026.
- Status atual: implantado em ambiente de homologação e em teste com usuários.
- Janela prevista: 15 a 20 usuários ativos.
- Perfil de uso: endêmico, com picos sazonais (por exemplo, no período de consolidação do PCA).

## 2. Pré-requisitos

- Docker Engine 24+.
- Docker Compose v2 (`docker compose`).
- Acesso de rede às portas:
`5173` (frontend), `5000` (backend API), `5432` (PostgreSQL) e `8088` (servidor de imagens).

Validação rápida:

```bash
docker --version
docker compose version
```

## 3. Variáveis de Ambiente (`.env`)

1. Copie o modelo:

```bash
cp .env.example .env
```

2. Edite o arquivo `.env` conforme o ambiente.

### 3.1 Chaves do `.env.example`

| Chave | Obrigatória | Descrição |
|---|---|---|
| `DB_USER` | Sim | Usuário do PostgreSQL usado pela aplicação. |
| `DB_PASSWORD` | Sim | Senha do PostgreSQL. |
| `DB_NAME` | Sim | Nome do banco principal da aplicação. |
| `JWT_KEY` | Sim | Segredo para assinatura de JWT (mínimo de 32 caracteres). |
| `JWT_ISSUER` | Sim | Emissor esperado do JWT no backend. |
| `JWT_AUDIENCE` | Sim | Audiência esperada do JWT no backend. |
| `ASPNETCORE_ENVIRONMENT` | Sim | Ambiente de execução (`Development`, `Staging`, `Production`). |
| `SERVER_HOST` | Sim (produção) | Host/IP para CORS e URLs externas. Ex.: `10.0.0.10` ou domínio. |
| `RESEND_API_KEY` | Sim | Token da API de e-mail (Resend). Sem essa chave o backend não inicializa. |

Observações:
- No Compose atual, a conexão de banco é montada automaticamente via `ConnectionStrings__DefaultConnection`.
- Se sua política DevOps exigir uma string única (`DB_CONNECTION_STRING`), mantenha esse segredo fora do repositório.

## 4. Implantação com Docker

### 4.1 Ambiente de desenvolvimento/local

Subir todos os serviços:

```bash
docker compose up -d --build
```

Verificar status e logs:

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

### 4.2 Ambiente de produção

Este projeto possui `docker-compose.prod.yml` com imagens prontas:

```bash
docker compose -f docker-compose.prod.yml up -d
```

Recomendações:
- Defina `.env` com valores de produção antes do `up`.
- Fixe versões de imagem (tag semântica) quando sair de homologação para produção estável.
- Coloque proxy reverso com TLS na frente dos serviços expostos.

## 5. Banco de Dados: Migrations e Seed

### 5.1 Comportamento padrão da aplicação

Ao iniciar, o backend executa:

1. `Database.MigrateAsync()` para aplicar migrations pendentes.
2. Seed inicial de centros, departamentos e usuários padrão, se as tabelas estiverem vazias.

No fluxo normal, não é necessário rodar migrations manualmente.

### 5.2 Execução manual (troubleshooting/pipeline)

Aplicar migrations:

```bash
docker compose run --rm --entrypoint sh backend-service -lc "export PATH=$PATH:/root/.dotnet/tools && dotnet tool restore && dotnet ef database update"
```

Criar nova migration:

```bash
docker compose run --rm --entrypoint sh backend-service -lc "export PATH=$PATH:/root/.dotnet/tools && dotnet tool restore && dotnet ef migrations add NomeDaMigration"
```

## 6. Escalabilidade e Operação

### 6.1 Escala para 15 a 20 usuários

- O perfil atual é suportado por um único host com Docker Compose.
- O gargalo principal tende a ser I/O de banco durante picos sazonais de solicitações.

### 6.2 Pico sazonal

- Monitore CPU e RAM de `backend-service` e `postgres-db`.
- Monitore latência e taxa de erro nas rotas de solicitações e relatórios.
- Planeje reforço de recursos nos períodos de maior demanda (ciclo PCA).

### 6.3 Escalonamento automático

Docker Compose não oferece autoscaling horizontal nativo por métrica.

Caminhos recomendados:
- Migrar para Kubernetes (HPA por CPU/memória e réplicas do backend).
- Usar ECS/Fargate com Auto Scaling e PostgreSQL gerenciado.
- Adicionar cache e/ou fila para amortecer picos de escrita e leitura.

## 7. Checklist Rápido de Implantação

1. Preencher `.env` com segredos reais.
2. Subir a stack com `docker compose up -d --build` (ou arquivo de produção).
3. Confirmar containers saudáveis com `docker compose ps`.
4. Validar Swagger (`/swagger`) e login no frontend.
5. Confirmar migrations e seed nos logs do backend.
6. Registrar data e status da implantação para rastreabilidade.
