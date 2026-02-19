# 🗃️ 02 - Gerenciamento do Banco de Dados

Este guia cobre operações de banco de dados com Entity Framework Core.

Para implantação e operação de ambiente, consulte `docs/06-IMPLANTACAO-DEVOPS.md`.

## Observação Importante

Sempre pare o container do backend antes de rodar comandos `dotnet ef`:

```bash
docker compose stop backend-service
```

## Migrations (Entity Framework)

As migrations são usadas para manter o schema do banco de dados sincronizado com o código C# (Entidades).

#### Criando uma Nova Migration
Use este comando quando for alterar uma entidade (adicionar uma propriedade, criar uma nova tabela, etc.).
```bash
docker compose run --rm --entrypoint sh backend-service
export PATH=$PATH:/root/.dotnet/tools
dotnet tool restore
dotnet ef migrations add NomeDaMigration
```

#### Aplicando as Migrations
Use este comando para aplicar todas as migrations pendentes ao banco de dados.
```bash
docker compose run --rm --entrypoint sh backend-service
export PATH=$PATH:/root/.dotnet/tools
dotnet tool restore
dotnet ef database update
```

## Seeders (Dados Iniciais)
O projeto possui seed inicial automático na subida do backend:

- `HasData` no `AppDbContext` popula **Categorias**.
- `DataSeeder.cs` popula **Centros, Departamentos e usuários padrão**.
- A lógica só executa quando as tabelas estão vazias.

Usuários padrão criados no seed:
- `admin@sistema.com`
- `solicitante@sistema.com`
- `gestor@sistema.com`

## Resetando o Banco de Dados (⚠️ Destrutivo)
Para apagar completamente o banco de dados e começar do zero, siga estes passos:

1.  **Pare e remova todos os contêineres:**
    ```bash
    docker compose down
    ```

2.  **Apague o volume do banco de dados:**
    ```bash
    docker volume rm compras-tcc-app_postgres-data
    ```
    *Dica: Confirme o nome do volume com `docker volume ls`.*

3.  **(Opcional) Limpe o histórico de migrations:** Se você quer recomeçar com uma única `InitialCreate`, apague a pasta `backend/src/Migrations`.
    ```bash
    # CUIDADO: Este comando apaga a pasta.
    sudo rm -rf backend/src/Migrations/
    ```

4.  **Crie a nova migration `InitialCreate` (se você apagou a pasta):**
    ```bash
    docker compose run --rm --entrypoint sh backend-service
    export PATH=$PATH:/root/.dotnet/tools
    dotnet tool restore
    dotnet ef migrations add InitialCreate
    ```

5.  **Suba o ambiente e aplique a migration:**
    ```bash
    # Sobe os containers (recriando o volume do banco vazio)
    docker compose up -d --build
    
    # Aplica a migration ao banco limpo
    docker compose run --rm --entrypoint sh backend-service
    export PATH=$PATH:/root/.dotnet/tools
    dotnet tool restore
    dotnet ef database update
    ```
