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

Atalhos principais:
- `Itens por Departamento`
- `Solicitações`
- `Catálogo`
- `Dashboards`
- `Relatórios`
- `Usuários`

Card de atenção:
- `Solicitações Pendentes` com botão `Ver Todas`.

## 3. Painel do Gestor

Rota base: `/gestor`

Abas operacionais:
- `Dashboard`
- `Itens por Departamento`
- `Solicitações`
- `Catálogo`
- `Relatórios`
- `Usuários`

## 4. Solicitações (análise e decisão)

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
1. Clique em `Ver Detalhes`.
2. No card de status, use o botão de lápis para `Alterar Status`.
3. Escolha o novo status.
4. Preencha `Justificativa` quando exigido.

### 4.3 Regras de negócio de status
Status possíveis na troca:
- `Pendente`
- `Aguardando Ajustes`
- `Aprovada`
- `Rejeitada`
- `Cancelada`

Regras:
- `Cancelada` e `Encerrada` são irreversíveis.
- Para mudança para `Aprovada`, justificativa pode ser opcional.
- Para mudança para outros status, justificativa é obrigatória.

## 5. Detalhes da solicitação

Rota: `/solicitacoes/:id`

A tela exibe:
- Requisitante e unidade/departamento.
- Data de criação.
- Tipo da solicitação.
- Status atual e observações.
- Abas: `Itens Solicitados`, `Insights`, `Histórico`.

Ações de gestão:
- Alterar status.
- Consultar histórico completo para auditoria.

## 6. Dashboard

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

Rota: `/gestor/catalogo`

### 8.1 Pesquisa e filtros
- `Pesquisar item`
- `Categoria`
- `Status` (`Ativo`/`Inativo`)
- `Filtros Avançados`
- `Ordem`
- Botões `Buscar` e `Limpar`

### 8.2 Criar item
1. Clique em `Criar`.
2. Preencha obrigatórios:
- `Nome *`
- `Catmat *`
- `Descrição *`
- `Categoria`
3. Opcional: imagem, especificação, preço sugerido, status.
4. Clique em `Criar`.

### 8.3 Editar item
1. Abra item e clique em `Editar`.
2. Atualize dados e imagem (`Selecionar` / `Remover`).
3. Clique em `Salvar` ou `Cancelar`.
4. Para excluir, clique em `Excluir`.

### 8.4 Histórico de item
- Disponível na aba `Histórico` da janela de detalhes.

## 9. Relatórios

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

Rota: `/gestor/usuarios`

### 10.1 Gestão de usuários
- Pesquisa por nome, e-mail, unidade e CPF.
- Filtro por `Ativos` e `Inativos`.

### 10.2 Criar usuário
1. Clique em `Novo Usuário`.
2. Preencha:
- `Nome`, `Email`, `CPF`, `Telefone`, `Senha`
- `Perfil de Acesso` (`Solicitante` ou `Gestor`)
- `Unidade Organizacional`
3. Clique em `Criar`.

### 10.3 Alterar perfil e status
- Ícone lápis: `Trocar Perfil`.
- Ícone power: `Desativar Usuário` / `Ativar Usuário`.

## 11. Configurações

Rota: `/configuracoes`

Abas:
- `Geral`
- `Solicitações`
- `Notificações`

### 11.1 Geral
- Campo: `Email de Contato Principal`.
- Ações: `Editar`, `Salvar Alterações`, `Cancelar`.

### 11.2 Notificações
- Campo: `Email de Notificações`.
- Ações: `Editar`, `Salvar Alterações`, `Cancelar`.

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

## 12. Status de solicitação (referência)

- `Pendente`: aguardando análise do gestor.
- `Aguardando Ajustes`: devolvida ao solicitante para correção.
- `Aprovada`: aceita pelo gestor.
- `Rejeitada`: negada pelo gestor.
- `Cancelada`: encerrada antecipadamente.
- `Encerrada`: arquivada automaticamente por ciclo anterior.

## 13. Boas práticas do gestor

- Registrar justificativas claras nas mudanças de status.
- Validar histórico antes de decisões de aprovação/rejeição.
- Revisar periodicamente os limites em `Configurações > Solicitações`.
- Utilizar relatórios e dashboards para decisões de planejamento e orçamento.
