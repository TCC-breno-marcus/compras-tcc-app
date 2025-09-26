/**
 * Categorias exibidas na view de Solicitação Geral.
 */
export const CATEGORY_ITEMS_GENERAL: string[] = [
  'Componentes Eletrônicos',
  'Ferramentas',
  'Reagentes Químicos',
  'Materiais de Laboratório',
  'Diversos',
]

/**
 * Categorias exibidas na view de Solicitação Patrimonial.
 */
export const CATEGORY_ITEMS_PATRIMONIALS: string[] = ['Eletrodomésticos', 'Mobiliário']

/**
 * Status de uma solicitação
 */
export const SOLICITATION_STATUS = [
  {
    id: 1,
    nome: 'Pendente',
    descricao: 'Solicitação recém-criada, aguardando a análise do gestor.',
    severity: 'info',
    icon: 'pi pi-clock',
  },
  {
    id: 2,
    nome: 'Aguardando Ajustes',
    descricao: 'Devolvida ao solicitante para correção ou mais informações.',
    severity: 'warn',
    icon: 'pi pi-exclamation-triangle',
  },
  {
    id: 3,
    nome: 'Aprovada',
    descricao: 'A solicitação foi aceita pelo gestor e seguirá para o próximo fluxo.',
    severity: 'success',
    icon: 'pi pi-check',
  },
  {
    id: 4,
    nome: 'Rejeitada',
    descricao: 'O pedido foi permanentemente negado pelo gestor.',
    severity: 'danger',
    icon: 'pi pi-times',
  },
  {
    id: 5,
    nome: 'Cancelada',
    descricao: 'Encerrada antecipadamente pelo solicitante ou gestor.',
    severity: 'danger',
    icon: 'pi pi-ban',
  },
  {
    id: 6,
    nome: 'Encerrada',
    descricao: 'Estado de arquivamento para solicitações de ciclos anteriores.',
    severity: 'contrast',
    icon: 'pi pi-lock',
  },
]
