# 06 - Expansão para Outros Centros (Uma Instância por Centro)

Este documento descreve como adaptar o sistema para outro Centro no modelo atual do projeto: **cada Centro utiliza sua própria instância do sistema**, com banco, usuários e configurações isolados.

Nesse cenário, a expansão é principalmente uma tarefa de **parametrização de dados iniciais** (Centro e Departamentos), sem necessidade de alterar arquitetura para multi-centro no mesmo banco.

## 1. Premissas do Modelo Atual

- Uma instância do sistema atende apenas um Centro.
- Cada Centro terá seu próprio ambiente (aplicação + banco de dados).
- Configurações (`/api/configuracao`) são globais da instância, o que é adequado para esse modelo.

## 2. Checklist de Adaptação

1. Provisionar nova instância (VM/container stack) para o Centro alvo.
2. Definir variáveis de ambiente próprias (`DB_*`, `JWT_*`, `SERVER_HOST`, etc.).
3. Ajustar seed de Centro e Departamentos no backend.
4. Ajustar usuários padrão (admin/gestor/solicitante), se necessário.
5. Subir aplicação e validar migração + seed.
6. Configurar parâmetros operacionais pelo painel de configurações.
7. Executar validação funcional mínima (login, cadastro, solicitação, relatório).

## 3. Arquivos a Ajustar

### 3.1 Centro padrão

Arquivo: `backend/src/Database/DataSeeder.cs`

Método: `SeedCentrosAsync`

Ajustar os campos do objeto `Centro`:
- `Nome`
- `Sigla`
- `Email`
- `Telefone`

### 3.2 Lista de Departamentos

Arquivo: `backend/src/Database/DataSeeder.cs`

Método: `SeedDepartamentosAsync`

- Substituir a lista `departamentos` pelos departamentos reais do novo Centro.
- Garantir que cada departamento esteja vinculado ao centro criado (`CentroId = <centro>.Id`).

### 3.3 Usuários iniciais (opcional, recomendado)

Arquivo: `backend/src/Database/DataSeeder.cs`

Método: `SeedUsersAsync`

- Atualizar credenciais padrão de `admin`, `gestor` e `solicitante` para o novo ambiente.
- Garantir que o solicitante seed use uma sigla de departamento existente na nova lista.
- Após a implantação, trocar senhas e desativar contas padrão, se aplicável.

## 4. Subida do Ambiente

Com `.env` preenchido para o novo Centro:

```bash
docker compose up -d --build
```

Validações iniciais:

```bash
docker compose ps
docker compose logs -f backend-service
```

No startup do backend, o fluxo esperado é:
1. Aplicação de migrations pendentes.
2. Seed inicial de Centro, Departamentos e usuários (somente se tabelas estiverem vazias).

## 5. Configuração Pós-Implantação

Após subir o sistema:

1. Acessar o painel de configurações.
2. Definir prazo de submissão.
3. Definir limites de itens/quantidade.
4. Definir e-mails institucionais de contato e notificações.

Essas configurações são específicas da instância e, portanto, específicas do Centro atendido.

## 6. Validação Funcional Mínima

Executar o seguinte roteiro de homologação:

1. Login com perfil gestor.
2. Login com perfil solicitante.
3. Criação de solicitação (geral e patrimonial, se aplicável).
4. Atualização de status pelo gestor.
5. Consulta de dashboard e relatórios.
6. Verificação dos departamentos exibidos no frontend.

## 7. Boas Práticas para Escalar o Processo

Para replicar em vários Centros com menos esforço manual:

- Manter um arquivo de parametrização por Centro (nome, sigla, contatos, lista de departamentos).
- Criar rotina/script de onboarding para popular seeds por ambiente.
- Padronizar checklist de implantação e validação.
- Versionar a documentação de cada rollout (data, centro, responsável, status).

## 8. Resumo

No estado atual do código, expandir para outros Centros é uma operação simples:
- criar nova instância,
- ajustar `Centro` e `Departamentos` no seed,
- subir o ambiente,
- configurar parâmetros operacionais.

Assim, reaproveita-se integralmente a base do sistema, com isolamento por Centro e baixa necessidade de mudança de código.
