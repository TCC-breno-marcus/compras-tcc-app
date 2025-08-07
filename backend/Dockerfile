# Use imagem base .NET 8 SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0

# Diretório de trabalho
WORKDIR /app/backend

# Instala dotnet-ef
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# Copia tudo
COPY . . 

# Restaura pacotes
RUN dotnet restore

# Constrói 
RUN dotnet build

# Expõe a porta
EXPOSE 8080

# Ponto de entrada
ENTRYPOINT ["dotnet", "watch", "run", "--urls", "http://*:8080"]
