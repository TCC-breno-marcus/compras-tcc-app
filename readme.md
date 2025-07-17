# BACKEND

Comando para rodar as migrations:
```
docker exec -it backend-service-container sh
export PATH=$PATH:/root/.dotnet/tools
dotnet ef database update
```