# 📦 Projeto Compras TCC

Este projeto utiliza `docker-compose` para orquestrar os serviços de **Backend**, **Banco de Dados** e **Frontend**.

---

## 🚀 Comandos Úteis

| Comando | Descrição |
|---------|------------|
| `docker-compose up` | Levanta todos os serviços em primeiro plano |
| `docker-compose up -d` | Levanta todos os serviços em background |
| `docker-compose stop` | Para os containers |
| `docker-compose down` | Para e remove os containers |
| `docker-compose up <nome_do_serviço>` | Levanta apenas um serviço e seus dependentes |
| `docker-compose build` | Reconstrói todas as imagens |
| `docker-compose build <nome_do_serviço>` | Reconstrói apenas a imagem do serviço especificado |
| `docker-compose up --build` | Sobe todos os serviços reconstruindo as imagens |

---

## ⚙️ Backend

### 🗂️ **Quando alterar uma entidade**

1️⃣ Crie uma nova migration:
```bash
docker exec -it backend-service-container sh
export PATH=$PATH:/root/.dotnet/tools
dotnet tool restore
dotnet ef migrations add NomeDaMigration
```

2️⃣ Rodar as migrations:
```bash
docker-compose run --rm --entrypoint sh backend-service
export PATH=$PATH:/root/.dotnet/tools
dotnet tool restore
dotnet ef database update
```