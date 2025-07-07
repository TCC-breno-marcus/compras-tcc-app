# 1. Use uma imagem base do Node.js (alpine é uma versão mais leve)
FROM node:20-alpine

# 2. Defina o diretório de trabalho dentro do container
WORKDIR /app/frontend

# 3. Copie o package.json e o package-lock.json ANTES do resto do código.
# Isso aproveita o cache do Docker. O npm install só rodará novamente se estes arquivos mudarem.
COPY package*.json ./

# 4. Instale as dependências do projeto
RUN npm install

# 5. Copie o resto do código do seu projeto para o container
COPY . .

# 6. Exponha a porta que o Vite usa
EXPOSE 5173

# 7. Defina o comando padrão para iniciar o servidor de desenvolvimento
CMD ["npm", "run", "dev", "--", "--host"]