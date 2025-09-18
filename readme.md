# 📦 Sistema de Gestão de Aquisições de Materiais (SIGAM)

![Badge .NET](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet)
![Badge Vue.js](https://img.shields.io/badge/Vue.js-3-4FC08D?logo=vue.js)
![Badge PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql)
![Badge Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker)

## 📝 Sobre o Projeto

Este projeto é um sistema web full-stack desenvolvido como Trabalho de Conclusão de Curso (TCC), projetado para otimizar o processo de **levantamento de demandas** para o Plano de Contratações Anual (PCA) em instituições de ensino, em especificamente o Centro de Ciências Exatas e Tecnologia (CCET) da Universidade Federal de Sergipe. A aplicação substitui o fluxo manual baseado em planilhas por uma plataforma centralizada, visando maior eficiência, integridade dos dados e transparência.

O sistema possui diferentes perfis de usuário (Solicitante, Gestor, Admin), um catálogo de itens categorizado e fluxos para criação, edição e visualização de solicitações e relatórios.

## ✨ Tecnologias Utilizadas

-   **Backend:** API RESTful com .NET 8, ASP.NET Core, Entity Framework Core
-   **Frontend:** Single Page Application (SPA) com Vue.js 3 (Composition API), Pinia e PrimeVue
-   **Banco de Dados:** PostgreSQL
-   **Autenticação:** JWT (JSON Web Tokens) com RBAC (Role-Based Access Control)
-   **Containerização:** Docker e Docker Compose

## 🚀 Começo Rápido

1.  **Pré-requisitos:** Docker e Docker Compose instalados.
2.  **Configuração:** Crie um arquivo `.env` na raiz do projeto (use o `.env.example` como base).
3.  **Execute:**
    ```bash
    docker-compose up -d --build
    ```
-   **Frontend:** `http://localhost:5173`
-   **Backend (Swagger):** `http://localhost:5000/swagger`

## 📚 Documentação

Para guias detalhados sobre configuração, execução e gerenciamento do banco de dados, consulte nossa documentação:

-   **[📄 01 - Setup e Execução do Projeto](./docs/01-SETUP.md)**
-   **[🗃️ 02 - Gerenciamento do Banco de Dados](./docs/02-DATABASE.md)**
