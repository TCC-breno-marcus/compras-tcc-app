# 06 - Manual do Solicitante

Este manual apresenta apenas as funcionalidades do perfil `Solicitante`.

## 1. Acesso ao sistema

### 1.1 Login
![alt text](imgs/image.png)

1. Acesse a tela `Acesso ao Sistema`.
2. Preencha os campos `Email` e `Senha`.
3. Clique em `Entrar`.

Mensagens de validação mais comuns:
- `O email é obrigatório.`
- `A senha é obrigatória.`

### 1.2 Navegação do solicitante
![alt text](imgs/image3.png)

Menu disponível para solicitante:
- `Início`
- `Solicitações`
- `Fale Conosco`

Menu do avatar:
![alt text](imgs/image-1.png)

- `Meu Perfil`
- `Alternar Tema`
- `Sair`

## 2. Página inicial
![alt text](imgs/image2.png)

Na home, o solicitante tem acesso aos atalhos:
- `Nova Solicitação` (botões `Geral` e `Patrimonial`)
- `Minhas Solicitações` (botão `Ver Histórico`)

Também possui o card:
- `Solicitações Aguardando Ajustes` com botão `Ver Todas`.

## 3. Criar solicitação

Tipos de solicitação:
- `Geral` (`/solicitacoes/criar/geral`)
- `Patrimonial` (`/solicitacoes/criar/patrimonial`)

### 3.1 Passo a passo
1. Na tela inicial, no card `Nova Solicitação`, escolha `Geral` ou `Patrimonial`.
<img width="483" height="204" alt="image" src="https://github.com/user-attachments/assets/c852379d-3215-4ea8-942a-4683a32c46c4" />
2. Na área `Buscar Itens`, localize os materiais desejados.
<img width="1906" height="900" alt="image" src="https://github.com/user-attachments/assets/222b57b5-c446-41b5-a581-308a48b5ae95" />
3. Use os filtros quando necessário:
- `Pesquisar item`
- `Categoria`
- `Ordem`
- `Filtros Avançados`
- `Buscar`
- `Limpar`
4. Clique em `Adicionar à Solicitação` (ou no ícone `+` do item).
5. Na área `Sua Solicitação`, preencha os campos dos itens:
- `Qtde.`
- `Preço Unitário`
- `Justificativa` (obrigatória para patrimonial)
6. Se a solicitação for `Geral`, preencha `Justificativa Geral`.
7. Clique em `Solicitar`.

### 3.2 Ações importantes durante a criação
- `Limpar Solicitação`: remove todo o conteúdo da solicitação em andamento.
- Ícone de lixeira no item: remove item individual.
- Ao clicar no item, você pode abrir o diálogo de detalhes e também adicionar por lá.

### 3.3 Regras de negócio da criação
- A solicitação deve ter pelo menos 1 item.
- Quantidade e preço unitário precisam ser maiores que zero.
- Em solicitação `Geral`, `Justificativa Geral` é obrigatória.
- Em solicitação `Patrimonial`, a justificativa é obrigatória item por item.
- Existe limite configurável de `Quantidade máxima por item`.
- Existe limite configurável de `Itens diferentes por solicitação`.
- Após o prazo final, envio e edição ficam bloqueados.

### 3.4 Regras de rascunho e descarte
- O sistema mantém apenas 1 solicitação em andamento no carrinho.
- Se você iniciar um tipo (`Geral`) e abrir o outro (`Patrimonial`), o sistema pede confirmação para descartar a solicitação atual.
- Ao tentar sair com alterações não salvas, o sistema alerta para evitar perda de dados.

## 4. Minhas Solicitações

Rota: `/solicitacoes`

<img width="518" height="205" alt="image" src="https://github.com/user-attachments/assets/7f92e85f-cb14-4292-8fe9-4ee0e7d7f4ee" />
<img width="1913" height="906" alt="image" src="https://github.com/user-attachments/assets/8345cfe8-e3f3-4164-a226-9a2b4a785ef1" />

### 4.1 Filtros da listagem
- `Código`
- `Data de Criação` (intervalo)
- `Tipo`
- `Status`
- Ordenação por data (`Ordenar por Data`, `Mais Recentes`, `Mais Antigos`)
- Botões `Buscar` e `Limpar`

### 4.2 Ações da listagem
- Na coluna `Ações`, clique em `Ver Detalhes`.
- Quando não houver resultados, os botões `Geral` e `Patrimonial` permitem iniciar nova solicitação.
- A paginação permite navegar entre páginas da listagem.

## 5. Detalhes da solicitação

Rota: `/solicitacoes/:id`

<img width="1913" height="898" alt="image" src="https://github.com/user-attachments/assets/016e079f-85a5-4c7b-9971-274c91bc3b2b" />

### 5.1 Informações exibidas
- Requisitante e unidade.
- Data de criação.
- Tipo da solicitação (`Geral` ou `Patrimonial`).
- Status atual.
- `Motivo informado` quando houver observações de status.

### 5.2 Abas disponíveis
- `Itens Solicitados`
- `Insights`
- `Histórico`

Na aba `Insights`, o sistema mostra indicadores e gráficos da solicitação atual.
Na aba `Histórico`, o sistema mostra as alterações com data, usuário e observações.

### 5.3 Edição pelo solicitante
O botão `Editar` aparece somente quando:
- a solicitação pertence à mesma unidade/departamento do solicitante logado;
- o status está em `Pendente` ou `Aguardando Ajustes`;
- o prazo para ajustes não expirou.

Passos:
1. Clique em `Editar`.
2. Atualize itens, quantidades, preços e justificativas.
3. Clique em `Salvar` para confirmar.
4. Use `Cancelar` para descartar.

## 6. Status da solicitação (referência)

- `Pendente`: aguardando análise.
- `Aguardando Ajustes`: devolvida para correção.
- `Aprovada`: aceita.
- `Rejeitada`: negada.
- `Cancelada`: encerrada antecipadamente.
- `Encerrada`: arquivada automaticamente por ciclo anterior.

## 7. Meu Perfil

Rota: `/perfil`

<img width="1911" height="901" alt="image" src="https://github.com/user-attachments/assets/e56d5b3e-71eb-4cb5-ab45-d0f74161d223" />

Funcionalidades:
- `Editar Perfil` para atualizar `Telefone` e `CPF`.
- `Salvar Alterações` e `Cancelar`.

Observações:
- `Nome` e `E-mail` não podem ser alterados por esta tela.

## 8. Fale Conosco

Rota: `/fale-conosco`

Passos:
1. Preencha `Nome`, `E-mail`, `Assunto` e `Mensagem`.
2. (Opcional) preencha `Telefone`.
3. Clique em `Enviar Mensagem`.
4. Use `Limpar` para reiniciar o formulário.

Validações:
- Nome obrigatório.
- E-mail obrigatório e válido.
- Assunto obrigatório.
- Mensagem obrigatória com mínimo de 10 caracteres.

## 9. Boas práticas

- Revise quantidade, preço e justificativas antes de enviar.
- Use filtros para localizar solicitações antigas mais rápido.
- Ao receber `Aguardando Ajustes`, priorize corrigir e reenviar antes do prazo.
- Consulte a aba `Histórico` para entender mudanças e observações.
