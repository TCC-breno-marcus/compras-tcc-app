# 02 - Gerenciamento do Banco de Dados

Este guia cobre operações de banco de dados com Entity Framework Core no ambiente Docker.

Para implantação e operação geral do ambiente, consulte `docs/06-IMPLANTACAO-DEVOPS.md`.

## 1. Migrations (Entity Framework Core)

As migrations mantêm o schema do banco sincronizado com o código C# (entidades).

### 1.1 Criando uma nova migration

Use quando alterar entidades (nova propriedade, tabela, relacionamento etc.).

```bash
docker compose run --rm --entrypoint sh backend-service -lc "export PATH=$PATH:/root/.dotnet/tools && dotnet tool restore && dotnet ef migrations add NomeDaMigration"
```

### 1.2 Aplicando migrations

Use para aplicar todas as migrations pendentes ao banco.

```bash
docker compose run --rm --entrypoint sh backend-service -lc "export PATH=$PATH:/root/.dotnet/tools && dotnet tool restore && dotnet ef database update"
```

## 2. Seeders (Dados Iniciais)

O projeto possui seed inicial automático na subida do backend:

- `HasData` no `AppDbContext` popula categorias.
- `DataSeeder.cs` popula centros, departamentos e usuários padrão.
- A lógica executa apenas quando as tabelas estão vazias.

Usuários padrão criados no seed:

- `admin@sistema.com`
- `solicitante@sistema.com`
- `gestor@sistema.com`

## 3. Resetando o Banco de Dados (Destrutivo)

Para apagar completamente o banco e começar do zero:

1. Pare e remova os containers:

```bash
docker compose down
```

2. Remova o volume do banco:

```bash
docker volume rm compras-tcc-app_postgres-data
```

Dica: confirme o nome com `docker volume ls`.

3. Opcional: limpe o histórico de migrations se quiser recomeçar com uma única `InitialCreate`:

```bash
rm -rf backend/Migrations
```

4. Gere novamente a migration inicial (se removeu a pasta):

```bash
docker compose run --rm --entrypoint sh backend-service -lc "export PATH=$PATH:/root/.dotnet/tools && dotnet tool restore && dotnet ef migrations add InitialCreate"
```

5. Suba o ambiente e aplique a migration:

```bash
docker compose up -d --build
docker compose run --rm --entrypoint sh backend-service -lc "export PATH=$PATH:/root/.dotnet/tools && dotnet tool restore && dotnet ef database update"
```
