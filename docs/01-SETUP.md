# 📄 01 - Setup e Execução do Projeto

Este guia detalha como configurar o ambiente e executar a aplicação.

## Pré-requisitos
- **Docker:** [Link para instalação](https://docs.docker.com/get-docker/)
- **Docker Compose:** Geralmente já vem com o Docker Desktop.
- **.NET SDK 8:** Necessário para o VS Code entender o código C# (IntelliSense).
- **Node.js e NPM:** Necessário para o VS Code entender o código Vue.js.

## Configuração Inicial
O projeto utiliza um arquivo `.env` na raiz para gerenciar segredos e variáveis de ambiente.

1.  Crie uma cópia do arquivo de exemplo:
    ```bash
    cp .env.example .env
    ```
2.  Abra o arquivo `.env` e, se necessário, ajuste as variáveis. As senhas e chaves secretas já vêm com valores seguros para o ambiente de desenvolvimento.

## Comandos do Docker Compose

| Comando | Descrição |
|---|---|
| `docker-compose up -d --build` | (Re)constrói as imagens e sobe todos os serviços em background. **Use sempre que houver alteração de código.** |
| `docker-compose up` | Sobe todos os serviços em primeiro plano, exibindo os logs. |
| `docker-compose down` | Para e remove os contêineres, mas **mantém os dados** do banco (no volume). |
| `docker-compose stop` | Apenas para os contêineres, sem removê-los. |
| `docker-compose logs -f` | Acompanha os logs de todos os serviços em tempo real. |
| `docker-compose logs -f <nome_do_servico>` | Acompanha os logs de um serviço específico (ex: `backend-service`). |
| `docker-compose build --no-cache` | Força a reconstrução de uma imagem sem usar o cache do Docker. Útil para resolver problemas de dependência. |
