# 03 - Manual do Usuário

Este manual descreve o uso do sistema pelo ponto de vista dos perfis `Solicitante` e `Gestor`, com base nas telas e ações disponíveis no frontend.

## 1. Perfis e Acesso

### 1.1 Perfis do sistema
- `Solicitante`: cria solicitações, acompanha status e realiza ajustes quando necessário.
- `Gestor`: analisa solicitações, altera status, gerencia catálogo, usuários, relatórios e configurações.
- `Admin`: possui acesso equivalente (ou superior) ao gestor e também às funcionalidades do solicitante.

### 1.2 Login
<img width="1903" height="897" alt="image" src="https://github.com/user-attachments/assets/5b6e9f16-fa53-44ba-8502-ea6d32f23be4" />

1. Acesse a tela `Acesso ao Sistema`.
2. Preencha os campos `Email` e `Senha`.
3. Clique em `Entrar`.
4. Em caso de campos vazios, o sistema exibe mensagens como:
- `O email é obrigatório.`
- `A senha é obrigatória.`

#### **Credenciais de Primeiro Acesso (Padrão)**
O sistema possui usuários pré-cadastrados no banco de dados para a configuração inicial. **Atenção:** Estas contas são de uso técnico e não devem ser utilizadas para as operações rotineiras após a implantação.
| **Perfil** | **E-mail** | **Senha** |
| --- | --- | --- |
| **Administrador Padrão** | `admin@sistema.com` | `123456` |
| **Gestor Padrão** | `gestor@sistema.com` | `123456` |
| **Solicitante Padrão** | `solicitante@sistema.com` | `123456` |

**Recomendação de Segurança:** Utilize o acesso de **Gestor Padrão** apenas no primeiro acesso para realizar o cadastro dos novos gestores e solicitantes reais do CCET. Uma vez criadas as contas oficiais, o uso das credenciais padrão deve ser descontinuado para garantir a auditabilidade e segurança do sistema.


### 1.3 Navegação principal
<img width="1911" height="908" alt="image" src="https://github.com/user-attachments/assets/9bb4d11e-87e1-4a32-8a2f-cc12dc477d43" />

- Menu lateral (desktop) ou menu em gaveta (mobile).
- Itens de menu por perfil:
- `Início`
- `Solicitações` (Solicitante/Admin)
- `Painel do Gestor` (Gestor/Admin)
- `Fale Conosco`
- `Configurações` (Gestor/Admin)

### 1.4 Menu do usuário (avatar no cabeçalho)
<img width="1913" height="899" alt="image" src="https://github.com/user-attachments/assets/07d12912-d147-449c-8e33-425cdcef3acf" />

- `Meu Perfil`
- `Alternar Tema`
- `Sair`

## 2. Funcionalidades comuns (todos os perfis)

### 2.1 Página inicial
<img width="1907" height="903" alt="image" src="https://github.com/user-attachments/assets/c5ed7f1d-a8d6-4ea8-b425-a9b334b8293a" />

- Exibe cartões (em vermelho na imagem) com atalhos conforme o perfil.
- Exibe componentes de atenção (em amarelo na imagem):
- Gestor/Admin: `Solicitações Pendentes`
- Solicitante: `Solicitações Aguardando Ajustes`

### 2.2 Meu Perfil (`/perfil`)
<img width="1911" height="901" alt="image" src="https://github.com/user-attachments/assets/e56d5b3e-71eb-4cb5-ab45-d0f74161d223" />

1. Acesse pelo avatar > `Meu Perfil`.
2. Clique em `Editar Perfil`.
3. Edite `Telefone` e `CPF`.
4. Clique em `Salvar Alterações` ou `Cancelar`.

Observações:
- `Nome` e `E-mail` não são editáveis diretamente pelo usuário.
- O botão `Salvar Alterações` só habilita quando houver mudanças.

### 2.3 Fale Conosco (`/fale-conosco`)
1. Preencha `Nome`, `E-mail`, `Assunto` e `Mensagem`.
2. Opcional: `Telefone`.
3. Clique em `Enviar Mensagem`.
4. Use `Limpar` para resetar o formulário.

Validações:
- Nome obrigatório.
- E-mail obrigatório e válido.
- Assunto obrigatório.
- Mensagem obrigatória com mínimo de 10 caracteres.

## 3. Manual do Solicitante

## 3.1 Criar solicitação

Existem dois tipos:
- `Geral` (`/solicitacoes/criar/geral`)
- `Patrimonial` (`/solicitacoes/criar/patrimonial`)

### Passo a passo
1. Acesse `Solicitações` > `Geral` ou `Bens Patrimoniais`.
2. Na área `Buscar Itens`, localize itens no catálogo:
- Campo `Pesquisar item`
- Filtro `Categoria`
- Botões `Ordem`, `Filtros Avançados`, `Buscar`, `Limpar`
3. Clique em `Adicionar à Solicitação` (ícone `+` ou botão no diálogo do item).
4. Na área `Sua Solicitação`, preencha os dados dos itens:
- `Qtde.`
- `Preço Unitário`
- `Justificativa` (obrigatória para patrimonial por item)
5. Para solicitação `Geral`, preencha `Justificativa Geral`.
6. Clique em `Solicitar`.

### Botões e ações importantes
- `Limpar Solicitação`: remove todos os itens da solicitação em andamento.
- Ícone de lixeira no item: remove item individual.

### Regras de validação
- Deve haver ao menos 1 item.
- Quantidade e preço devem ser maiores que zero.
- Em `Geral`: `Justificativa Geral` é obrigatória.
- Em `Patrimonial`: a justificativa é obrigatória item por item (cada item precisa da sua própria justificativa).
- Respeita limites configuráveis:
- `Quantidade máxima por item` (definida em Configurações pelo gestor).
- `Itens diferentes por solicitação` (limite total de itens diferentes por solicitação, definido em Configurações pelo gestor).
- Respeita prazo de submissão: após vencimento, envio e edição ficam bloqueados.

### Regras de negócio adicionais (criação)
- O sistema permite apenas 1 solicitação em andamento por vez no carrinho de criação.
- Se o usuário iniciar uma solicitação de um tipo (`Geral`) e tentar abrir a criação do outro tipo (`Patrimonial`), o sistema solicita confirmação para descartar a solicitação em andamento.
- Ao sair da tela com solicitação em andamento, o sistema alerta sobre descarte de alterações não salvas.

## 3.2 Minhas Solicitações (`/solicitacoes`)

### Filtros e pesquisa
- `Código`
- `Data de Criação` (intervalo)
- `Tipo`
- `Status`
- Ordenação com botão (`Ordenar por Data`, `Mais Recentes`, `Mais Antigos`)
- Botões: `Buscar` e `Limpar`

### Visualização
1. Na coluna `Ações`, clique em `Ver Detalhes` (ícone olho).
2. Navegue pelas abas:
- `Itens Solicitados`
- `Insights`
- `Histórico`

## 3.3 Detalhes da Solicitação (`/solicitacoes/:id`)

### Edição pelo solicitante
O botão `Editar` aparece quando:
- a solicitação pertence à mesma unidade/departamento do solicitante logado,
- o status permite edição (`Pendente` ou `Aguardando Ajustes`),
- o prazo para ajustes não expirou.

### Passos de ajuste
1. Clique em `Editar`.
2. Altere itens, quantidades, preços, justificativas e, no tipo geral, `Justificativa Geral`.
3. Clique em `Salvar` para confirmar.
4. Use `Cancelar` para descartar mudanças.

### Status e motivos
- A tela mostra o status atual e, quando existir, `Motivo informado`.
- O histórico exibe alterações com data, usuário e observações.

## 4. Manual do Gestor

Acesso pelo menu `Painel do Gestor`.

## 4.1 Aba Solicitações (`/gestor/solicitacoes`)

### Filtros disponíveis
- `Código`
- `Data de Criação`
- `Solicitante`
- `Departamento`
- `Tipo`
- `Status`
- Ordenação por data
- Botões `Buscar` e `Limpar`

### Ações
1. Clique em `Ver Detalhes` para abrir uma solicitação.
2. No card de status, use o botão de lápis para `Alterar Status`.
3. Escolha novo status e informe `Justificativa` quando exigido.

Status disponíveis na troca:
- `Pendente`
- `Aguardando Ajustes`
- `Aprovada`
- `Rejeitada`
- `Cancelada`

Observação:
- `Cancelada` e `Encerrada` são irreversíveis (botão de alteração fica desabilitado).
- Para mudança para `Aprovada`, a justificativa pode ser opcional.
- Para mudança para outros status (`Pendente`, `Aguardando Ajustes`, `Rejeitada`, `Cancelada`), a justificativa é obrigatória.

## 4.2 Aba Dashboard (`/gestor/dashboard`)

Exibe KPIs e gráficos:
- `Total de Solicitações`
- `Departamentos Solicitantes`
- `Total de Itens Únicos`
- `Total de Unidades Solicitadas`
- `Valor Total Estimado`
- `Custo Médio por Solicitação`

Gráficos com alternância por `Toggle`:
- Absoluto x percentual (`R$`/`%` ou `Qtd`/`%`)

## 4.3 Aba Itens por Departamento (`/gestor/departamento`)

### Recursos
- Filtros: `Pesquisar item`, `Departamento`, ordenação.
- Alternância por tipo: `Itens Gerais` e `Itens Patrimoniais`.
- Exportação: botão `Exportar` com opções:
- `Excel (.xlsx)`
- `CSV (.csv)`

### Ações
- Clique no cartão do item para ver detalhes em popover.
- Use `Buscar` e `Limpar` para refinar resultado.

## 4.4 Aba Catálogo (`/gestor/catalogo`)

### Pesquisa e filtros
- `Pesquisar item`
- `Categoria`
- `Status` (`Ativo`/`Inativo`)
- `Filtros Avançados`
- `Ordem`
- Botões `Buscar` e `Limpar`

### Criar item
1. Clique em `Criar`.
2. Preencha os campos obrigatórios:
- `Nome *`
- `Catmat *`
- `Descrição *`
- `Categoria`
3. Opcional: imagem, especificação, preço sugerido, status.
4. Clique em `Criar`.

### Editar item
1. Abra um item e clique em `Editar`.
2. Altere dados e, se necessário, imagem (`Selecionar` / `Remover`).
3. Clique em `Salvar` ou `Cancelar`.
4. Para exclusão, clique em `Excluir`.

### Histórico de item
- Disponível na aba `Histórico` da janela de detalhes (para gestor/admin).

## 4.5 Aba Relatórios (`/gestor/relatorios`)

1. Em `Gerador de Relatórios`, selecione:
- `Período`
- `Tipo de Relatório`
2. Clique em `Gerar Relatório`.

Tipos:
- `Gastos por Centro`
- `Consumo por Categoria`

Saída:
- Gráfico + tabela de detalhamento correspondente.

## 4.6 Aba Usuários (`/gestor/usuarios`)

### Gestão de usuários
- Pesquisa por nome, e-mail, unidade ou CPF.
- Filtro por `Ativos` e `Inativos`.

### Criar usuário
1. Clique em `Novo Usuário`.
2. Preencha:
- `Nome`, `Email`, `CPF`, `Telefone`, `Senha`
- `Perfil de Acesso` (`Solicitante` ou `Gestor`)
- `Unidade Organizacional`
3. Clique em `Criar`.

### Alterar perfil e status
- Ícone lápis: `Trocar Perfil`.
- Ícone power: `Desativar Usuário` / `Ativar Usuário`.

## 4.7 Configurações (`/configuracoes`)

Abas:
- `Geral`
- `Solicitações`
- `Notificações`

### Geral
- Campo: `Email de Contato Principal`
- Ações: `Editar`, `Salvar Alterações`, `Cancelar`

### Notificações
- Campo: `Email de Notificações`
- Ações: `Editar`, `Salvar Alterações`, `Cancelar`

### Solicitações
- `Prazo final para criação/edição`
- `Quantidade máxima por item`
- `Itens diferentes por solicitação`
- Ações: `Editar`, `Salvar Alterações`, `Cancelar`
- Rotina administrativa: `Encerrar Anos Anteriores`

Regras de negócio dessas configurações:
- Alterações entram em vigor imediatamente para novas ações no sistema.
- Solicitações já criadas não são retroativamente alteradas por mudanças de configuração.
- O encerramento de anos anteriores arquiva solicitações antigas e gera status `Encerrada` (irreversível).

## 5. Status de Solicitação (referência)

- `Pendente`: aguardando análise do gestor.
- `Aguardando Ajustes`: devolvida ao solicitante para correção.
- `Aprovada`: aceita pelo gestor.
- `Rejeitada`: negada pelo gestor.
- `Cancelada`: encerrada antecipadamente.
- `Encerrada`: arquivada automaticamente por ciclo anterior.

## 6. Boas práticas de uso

- Sempre revisar quantidade, preço e justificativas antes de enviar.
- Utilizar filtros para localizar solicitações e itens rapidamente.
- Para decisões de status (gestor), registrar justificativa clara para rastreabilidade.
- Usar aba `Histórico` para auditoria de mudanças em solicitações e itens.
