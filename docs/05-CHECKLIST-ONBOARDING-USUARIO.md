# 05 - Checklist Operacional de Onboarding

Use este checklist para habilitar e validar o uso do sistema por perfil.

## 1. Checklist inicial (todos os perfis)
- [ ] Acessou `/login`.
- [ ] Conseguiu autenticar com `Email` e `Senha`.
- [ ] Visualizou o menu lateral (desktop) ou menu em gaveta (mobile).
- [ ] Abriu avatar e confirmou ações: `Meu Perfil`, `Alternar Tema`, `Sair`.
- [ ] Acessou `Meu Perfil` e validou dados pessoais.
- [ ] Acessou `Fale Conosco` e validou envio de mensagem.

## 2. Checklist do Solicitante

### 2.1 Criar solicitação
- [ ] Acessou `Solicitações` > `Geral`.
- [ ] Acessou `Solicitações` > `Bens Patrimoniais`.
- [ ] Usou filtros de catálogo (`Pesquisar item`, `Categoria`, `Buscar`, `Limpar`).
- [ ] Adicionou item com `Adicionar à Solicitação`.
- [ ] Preencheu `Qtde.` e `Preço Unitário`.
- [ ] Preencheu `Justificativa Geral` (tipo geral).
- [ ] Preencheu `Justificativa` por item (tipo patrimonial).
- [ ] Enviou com `Solicitar`.
- [ ] Validou regra de troca de tipo (`Geral` <-> `Patrimonial`) com confirmação de descarte da solicitação em andamento.

### 2.2 Acompanhamento e ajustes
- [ ] Acessou `Minhas Solicitações`.
- [ ] Filtrou por `Código`, `Data de Criação`, `Tipo` e `Status`.
- [ ] Abriu `Ver Detalhes`.
- [ ] Navegou nas abas `Itens Solicitados`, `Insights`, `Histórico`.
- [ ] Entrou em modo `Editar` quando permitido.
- [ ] Salvou alterações com `Salvar`.

### 2.3 Validações esperadas
- [ ] Sistema impede envio sem itens.
- [ ] Sistema impede quantidade/preço zerados.
- [ ] Sistema exige justificativas conforme tipo.
- [ ] Sistema respeita prazo de envio/ajustes.
- [ ] Sistema aplica limite de itens diferentes por solicitação conforme configuração.
- [ ] Sistema aplica limite de quantidade por item conforme configuração.

## 3. Checklist do Gestor

### 3.1 Solicitações
- [ ] Acessou `Painel do Gestor` > `Solicitações`.
- [ ] Filtrou por `Solicitante`, `Departamento`, `Tipo` e `Status`.
- [ ] Abriu solicitação com `Ver Detalhes`.
- [ ] Alterou status via botão de lápis.
- [ ] Registrou justificativa em mudança de status.
- [ ] Confirmou bloqueio de alteração para status irreversíveis.
- [ ] Validou que `Aprovada` permite alteração sem justificativa obrigatória.
- [ ] Validou que outros status exigem justificativa obrigatória.

### 3.2 Dashboard e análise
- [ ] Acessou `Dashboard`.
- [ ] Validou cards de KPI.
- [ ] Alternou gráficos entre absoluto e percentual.

### 3.3 Itens por Departamento
- [ ] Acessou `Itens por Departamento`.
- [ ] Alternou `Itens Gerais` e `Itens Patrimoniais`.
- [ ] Usou filtros e pesquisa.
- [ ] Exportou relatório em `Excel (.xlsx)`.
- [ ] Exportou relatório em `CSV (.csv)`.

### 3.4 Catálogo
- [ ] Acessou `Catálogo`.
- [ ] Criou item com `Criar`.
- [ ] Editou item com `Editar` > `Salvar`.
- [ ] Removeu imagem (`Remover`) e testou upload (`Selecionar`).
- [ ] Excluiu item com `Excluir`.
- [ ] Consultou aba `Histórico` do item.

### 3.5 Relatórios
- [ ] Acessou `Relatórios`.
- [ ] Selecionou `Período`.
- [ ] Selecionou `Tipo de Relatório`.
- [ ] Clicou em `Gerar Relatório`.
- [ ] Validou gráfico e tabela de detalhamento.

### 3.6 Usuários
- [ ] Acessou `Usuários`.
- [ ] Criou usuário via `Novo Usuário`.
- [ ] Alterou perfil com ícone lápis.
- [ ] Desativou/ativou usuário com ícone power.
- [ ] Testou filtro `Ativos`/`Inativos`.

### 3.7 Configurações
- [ ] Acessou aba `Geral` e editou `Email de Contato Principal`.
- [ ] Acessou aba `Notificações` e editou `Email de Notificações`.
- [ ] Acessou aba `Solicitações` e atualizou:
- [ ] `Prazo final para criação/edição`
- [ ] `Quantidade máxima por item`
- [ ] `Itens diferentes por solicitação`
- [ ] Executou (quando aplicável) `Encerrar Anos Anteriores`.
- [ ] Confirmou que mudanças de configuração valem imediatamente para novas operações.
- [ ] Confirmou que solicitações já existentes não são alteradas retroativamente.

## 4. Critérios de aceite do onboarding
- [ ] Usuário executa seu fluxo principal sem bloqueios.
- [ ] Campos obrigatórios e mensagens de erro estão claros.
- [ ] Permissões por perfil estão consistentes com o esperado.
- [ ] Histórico/auditoria registra alterações relevantes.
