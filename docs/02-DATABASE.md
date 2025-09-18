# 🗃️ 02 - Gerenciamento do Banco de Dados

Este guia cobre as operações de banco de dados usando o Entity Framework Core.

**Importante:** Sempre pare o contêiner do back-end antes de rodar comandos `dotnet ef` para evitar erros de arquivos bloqueados.
```bash
docker-compose stop backend-service
```

## Migrations (Entity Framework)

As migrations são usadas para manter o schema do banco de dados sincronizado com o código C# (Entidades).

#### Criando uma Nova Migration
Use este comando quando for alterar uma entidade (adicionar uma propriedade, criar uma nova tabela, etc.).
```bash
docker-compose run --rm --entrypoint sh backend-service
export PATH=$PATH:/root/.dotnet/tools
dotnet tool restore
dotnet ef migrations add NomeDaMigration
```

#### Aplicando as Migrations
Use este comando para aplicar todas as migrations pendentes ao banco de dados.
```bash
docker-compose run --rm --entrypoint sh backend-service
export PATH=$PATH:/root/.dotnet/tools
dotnet tool restore
dotnet ef database update
```

## Seeders (Dados Iniciais)
O projeto está configurado para popular o banco de dados com dados essenciais (Centros, Departamentos, Usuários Padrão) automaticamente na primeira vez que a aplicação sobe em ambiente de desenvolvimento.

-   A configuração de `HasData` no `AppDbContext` popula as **Categorias**.
-   A classe `DataSeeder.cs` popula **Centros, Departamentos e Usuários Padrão**.
-   Esta lógica só é executada se a tabela correspondente estiver vazia.

## Resetando o Banco de Dados (⚠️ Destrutivo)
Para apagar completamente o banco de dados e começar do zero, siga estes passos:

1.  **Pare e remova todos os contêineres:**
    ```bash
    docker-compose down
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
    docker-compose run --rm --entrypoint sh backend-service
    export PATH=$PATH:/root/.dotnet/tools
    dotnet tool restore
    dotnet ef migrations add InitialCreate
    ```

5.  **Suba o ambiente e aplique a migration:**
    ```bash
    # Sobe os containers (recriando o volume do banco vazio)
    docker-compose up -d --build
    
    # Aplica a migration ao banco limpo
    docker-compose run --rm --entrypoint sh backend-service
    export PATH=$PATH:/root/.dotnet/tools
    dotnet tool restore
    dotnet ef database update
    ```