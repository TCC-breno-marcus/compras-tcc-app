# 07 - Manual do Gestor

Este manual descreve o uso do sistema pelo ponto de vista do perfil `Gestor`, incluindo visão macro de acesso e operação administrativa completa.

## 1. Perfis e acesso

### 1.1 Perfis do sistema
- `Solicitante`: cria solicitações, acompanha status e realiza ajustes quando necessário.
- `Gestor`: analisa solicitações, altera status, gerencia catálogo, usuários, relatórios e configurações.
- `Admin`: possui acesso equivalente (ou superior) ao gestor e também às funcionalidades do solicitante.

### 1.2 Login
<img width="1903" height="897" alt="image" src="https://github.com/user-attachments/assets/5b6e9f16-fa53-44ba-8502-ea6d32f23be4" />

1. Acesse a tela `Acesso ao Sistema`.
2. Preencha os campos `Email` e `Senha`.
3. Clique em `Entrar`.

Mensagens de validação mais comuns:
- `O email é obrigatório.`
- `A senha é obrigatória.`

### 1.3 Credenciais de primeiro acesso (padrão)
O sistema possui usuários pré-cadastrados no banco de dados para configuração inicial.

| Perfil | E-mail | Senha |
| --- | --- | --- |
| Administrador Padrão | `admin@sistema.com` | `123456` |
| Gestor Padrão | `gestor@sistema.com` | `123456` |
| Solicitante Padrão | `solicitante@sistema.com` | `123456` |

Recomendação de segurança:
- Utilize o acesso de `Gestor Padrão` apenas para cadastro inicial de usuários reais.
- Após implantação, descontinue o uso das credenciais padrão.

### 1.4 Navegação do gestor
<img width="1911" height="908" alt="image" src="https://github.com/user-attachments/assets/9bb4d11e-87e1-4a32-8a2f-cc12dc477d43" />

Menu principal do gestor:
- `Início`
- `Painel do Gestor`
- `Fale Conosco`
- `Configurações`

Menu do avatar:
<img width="1913" height="899" alt="image" src="https://github.com/user-attachments/assets/07d12912-d147-449c-8e33-425cdcef3acf" />

- `Meu Perfil`
- `Alternar Tema`
- `Sair`

## 2. Página inicial do gestor
<img width="1907" height="903" alt="image" src="https://github.com/user-attachments/assets/c5ed7f1d-a8d6-4ea8-b425-a9b334b8293a" />

Atalhos para as principais funcionalidades:
  - `Itens por Departamento`
  - `Solicitações`
  - `Catálogo`
  - `Dashboards`
  - `Relatórios`
  - `Usuários`

Card de atenção:
- `Solicitações Pendentes` com botão `Ver Todas`.
  - Representa as solicitações que o gestor precisa realizar análise para aprovação ou não.   

## 3. Painel do Gestor

Rota base: `/gestor`

Abas operacionais:
  - `Itens por Departamento`: lista de itens solicitados e suas respectivas quantidades, com agrupamento por departamento.
  - `Solicitações`: listagem de todas as solicitações.
  - `Catálogo`: gerenciamento de itens (criação, edição e remoção).
  - `Dashboards`: acesso a insights e métricas gerais de solicitações do ano atual.
  - `Relatórios`: extração de relatórios.
  - `Usuários`: gerenciamento de usuários (criação, alteração de papel).

## 4. Solicitações (análise e decisão)
<img width="1908" height="903" alt="image" src="https://github.com/user-attachments/assets/6506b7a5-6215-46a1-affa-ce9442eeb74e" />

Lista de todas as solicitações feitas.

Rota: `/gestor/solicitacoes`

### 4.1 Filtros disponíveis
- `Código`
- `Data de Criação`
- `Solicitante`
- `Departamento`
- `Tipo`
- `Status`
- Ordenação por data
- Botões `Buscar` e `Limpar`

### 4.2 Ações
1. Clique em `Ver Detalhes` para acessar uma solicitação específica.

## 5. Detalhes da solicitação
<img width="1907" height="902" alt="image" src="https://github.com/user-attachments/assets/9d494a8c-811b-46e0-9393-93df9009976c" />

Rota: `/solicitacoes/:id`

A tela exibe:
- Requisitante e unidade/departamento.
- Data de criação.
- Tipo da solicitação.
- Status atual e observações.
- Abas: `Itens Solicitados`, `Insights`, `Histórico`.

### 5.1 Ações de gestão
Alterar status:
1. No card de status, use o botão com ícone de lápis para `Alterar Status`.
2. Escolha o novo status.
3. Preencha `Justificativa` quando exigido.
  - Regras:
    - `Cancelada` e `Encerrada` são irreversíveis.
    - Para mudança para `Aprovada`, justificativa pode ser opcional.
    - Para mudança para outros status, justificativa é obrigatória.
    - 
Aba "Insights":
  - Acesse para visualizar algumas métricas e gráficos para auxílio na análise da solicitação.
  - <img width="1912" height="900" alt="image" src="https://github.com/user-attachments/assets/84addea6-8126-4062-827c-fdef063afb01" />


Aba "Histórico":
  - Acesse para consultar histórico completo para auditoria.

## 6. Dashboard
<img width="1909" height="906" alt="image" src="https://github.com/user-attachments/assets/6a21b6dc-c1b3-4da4-b80d-f209bf249146" />

Rota: `/gestor/dashboard`

KPIs e indicadores:
- `Total de Solicitações`
- `Departamentos Solicitantes`
- `Total de Itens Únicos`
- `Total de Unidades Solicitadas`
- `Valor Total Estimado`
- `Custo Médio por Solicitação`

Gráficos:
- Itens de maior valor.
- Itens mais solicitados.
- Valor por departamento.
- Status das solicitações.

Recursos:
- Alternância entre absoluto e percentual (`R$`/`%`, `Qtd`/`%`).

## 7. Itens por Departamento
<img width="1909" height="904" alt="image" src="https://github.com/user-attachments/assets/65494275-76df-4c78-a8ee-8f0eda5a5c06" />

Rota: `/gestor/departamento`

### 7.1 Funcionalidades
- Filtros por item/departamento.
- Ordenação.
- Alternância entre `Itens Gerais` e `Itens Patrimoniais`.

### 7.2 Exportação
Botão `Exportar` com opções:
- `Excel (.xlsx)`
- `CSV (.csv)`

### 7.3 Ações
- Clicar no item para abrir detalhes em popover.
- Usar `Buscar` e `Limpar` para refinar listagem.

## 8. Catálogo
<img width="1911" height="903" alt="image" src="https://github.com/user-attachments/assets/5b41110a-b30f-4989-948a-36d630be6527" />

Rota: `/gestor/catalogo`

### 8.1 Pesquisa e filtros
- `Pesquisar item`
- `Categoria`
- `Status` (`Ativo`/`Inativo`)
- `Filtros Avançados`
- `Ordem`
- Botões `Buscar` e `Limpar`

### 8.2 Criar item
<img width="1905" height="901" alt="image" src="https://github.com/user-attachments/assets/575f5f6b-ea72-4a80-82b4-b26e13aeb4ad" />

1. Clique em `Criar`.
2. Preencha obrigatórios:
  - `Nome *`
  - `Catmat *`
  - `Descrição *`
  - `Categoria`
3. Opcional: imagem, especificação, preço sugerido.
4. Clique em `Criar`.

### 8.3 Editar item
<img width="787" height="511" alt="image" src="https://github.com/user-attachments/assets/eb2b4ba5-4889-4930-b992-599fdea5fc56" />

1. Abra item e clique em `Editar`.
2. Atualize dados e imagem (`Selecionar` / `Remover`).
3. Clique em `Salvar` ou `Cancelar`.
4. Para excluir, clique em `Excluir`.

### 8.4 Histórico de item
- Disponível na aba `Histórico` da janela de detalhes.

## 9. Relatórios
<img width="1905" height="905" alt="image" src="https://github.com/user-attachments/assets/e38c3807-45f2-4d03-93fb-042d780754be" />

Rota: `/gestor/relatorios`

Passos:
1. Selecione `Período`.
2. Selecione `Tipo de Relatório`.
3. Clique em `Gerar Relatório`.

Tipos:
- `Gastos por Centro`
- `Consumo por Categoria`

Saída:
- Gráfico e tabela de detalhamento.

## 10. Usuários
<img width="1908" height="904" alt="image" src="https://github.com/user-attachments/assets/19aa6731-8c7c-4a68-81ff-6a737482f979" />

Rota: `/gestor/usuarios`

### 10.1 Gestão de usuários
- Pesquisa por nome, e-mail, unidade e CPF.
- Filtro por `Ativos` e `Inativos`.

### 10.2 Criar usuário
<img width="1908" height="899" alt="image" src="https://github.com/user-attachments/assets/ab8322b5-1b13-415d-808a-3fbbf20e6f4b" />

1. Clique em `Novo Usuário`.
2. Preencha:
- `Nome`, `Email`, `CPF`, `Telefone`, `Senha`
- `Perfil de Acesso` (`Solicitante` ou `Gestor`)
- `Unidade Organizacional`
3. Clique em `Criar`.

### 10.3 Alterar perfil e status
<img width="1907" height="902" alt="image" src="https://github.com/user-attachments/assets/692a8b0f-cd1c-4f2b-802e-eae3b5a971ef" />

- Ícone lápis: `Trocar Perfil`.
- Ícone power: `Desativar Usuário` / `Ativar Usuário`.

## 11. Configurações
<img width="1904" height="903" alt="image" src="https://github.com/user-attachments/assets/54fe6540-21a6-4462-86de-2e3721ad8d0d" />

Rota: `/configuracoes`

Abas:
- `Geral`
- `Solicitações`
- `Notificações`

### 11.1 Geral
- Campo: `Email de Contato Principal`.
- Ações: `Editar`, `Salvar Alterações`, `Cancelar`.
- Status: Funcionalidade incompleta pois depende de integração com email.

### 11.2 Notificações
- Campo: `Email de Notificações`.
- Ações: `Editar`, `Salvar Alterações`, `Cancelar`.
- Status: Funcionalidade incompleta pois depende de integração com email.

### 11.3 Solicitações
Parâmetros:
- `Prazo final para criação/edição`
- `Quantidade máxima por item`
- `Itens diferentes por solicitação`

Ações:
- `Editar`, `Salvar Alterações`, `Cancelar`
- `Encerrar Anos Anteriores`

Regras de negócio:
- Alterações entram em vigor imediatamente para novas operações.
- Solicitações já criadas não são alteradas retroativamente.
- Encerramento de anos anteriores gera status `Encerrada` (irreversível).

## 12. Fale Conosco
<img width="1906" height="900" alt="image" src="https://github.com/user-attachments/assets/e035ff5a-6505-4b20-91ba-fe597e758f2d" />

Rota: `/fale-conosco`

Status atual:
- Funcionalidade ainda não está integrada ao backend.
- Não há abertura de chamado/protocolo dentro do sistema.
- Integração de atendimento está prevista para evolução futura.
- A página também serve como referência de dúvidas frequentes e regras de negócio (FAQ).

Uso no contexto de gestão:
- A tela pode ser usada apenas como referência de interface no estado atual.

## 13. Status de solicitação (referência)

- `Pendente`: aguardando análise do gestor.
- `Aguardando Ajustes`: devolvida ao solicitante para correção.
- `Aprovada`: aceita pelo gestor.
- `Rejeitada`: negada pelo gestor.
- `Cancelada`: cancelada pelo gestor (irreversível).
- `Encerrada`: arquivada automaticamente pelo sistema por ser de anos anteriores (irreversível).

## 14. Boas práticas do gestor

- Registrar justificativas claras nas mudanças de status.
- Validar histórico antes de decisões de aprovação/rejeição.
- Revisar periodicamente os limites em `Configurações > Solicitações`.
- Utilizar relatórios e dashboards para decisões de planejamento e orçamento.
