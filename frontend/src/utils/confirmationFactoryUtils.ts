import type { ConfirmationOptions } from 'primevue/confirmationoptions'

/**
 * Configuração padrão para um diálogo de confirmação de SALVAR.
 */
export const SAVE_CONFIRMATION: ConfirmationOptions = {
  message: 'As alterações serão salvas. Deseja continuar?',
  header: 'Salvar Alterações?',
  icon: 'pi pi-exclamation-triangle',
  rejectProps: {
    label: 'Cancelar',
    severity: 'secondary',
    text: true,
    icon: 'pi pi-times',
    size: 'small',
  },
  acceptProps: {
    label: 'Salvar',
    icon: 'pi pi-save',
    size: 'small',
    severity: 'success',
  },
}

/**
 * Configuração padrão para um diálogo de confirmação de DELETAR IMAGEM.
 */
export const DEL_IMAGE_CONFIRMATION: ConfirmationOptions = {
  message: 'Tem certeza que deseja remover a imagem atual? O item ficará sem imagem.',
  header: 'Remover Imagem?',
  icon: 'pi pi-trash',
  acceptProps: {
    label: 'Remover',
    icon: 'pi pi-trash',
    size: 'small',
    severity: 'danger',
  },
  rejectProps: {
    label: 'Cancelar',
    severity: 'secondary',
    text: true,
    icon: 'pi pi-times',
    size: 'small',
  },
}

/**
 * Configuração padrão para um diálogo de confirmação de DELETAR ITEM.
 */
export const DEL_ITEM_CONFIRMATION: ConfirmationOptions = {
  message: 'Tem certeza que deseja excluir este item?',
  header: 'Excluir Item?',
  icon: 'pi pi-trash',
  acceptProps: {
    label: 'Excluir',
    icon: 'pi pi-trash',
    size: 'small',
    severity: 'danger',
  },
  rejectProps: {
    label: 'Cancelar',
    severity: 'secondary',
    text: true,
    icon: 'pi pi-times',
    size: 'small',
  },
}

/**
 * Configuração padrão para um diálogo de confirmação de CANCELAR EDIÇÂO.
 */
export const CANCEL_CONFIRMATION: ConfirmationOptions = {
  message: `As alterações feitas serão descartadas. Deseja continuar?`,
  header: 'Descartar Alterações?',
  icon: 'pi pi-exclamation-triangle',
  rejectProps: {
    label: 'Continuar Editando',
    severity: 'secondary',
    text: true,
    icon: 'pi pi-arrow-left',
    size: 'small',
  },
  acceptProps: {
    label: 'Descartar',
    icon: 'pi pi-trash',
    size: 'small',
    severity: 'danger',
  },
}

/**
 * Configuração padrão para um diálogo de confirmação de FECHAR MODAL.
 */
export const CLOSE_CONFIRMATION: ConfirmationOptions = {
  message: `As alterações feitas serão descartadas. Deseja continuar?`,
  header: 'Descartar Alterações?',
  icon: 'pi pi-exclamation-triangle',
  rejectProps: {
    label: 'Continuar Editando',
    severity: 'secondary',
    text: true,
    icon: 'pi pi-arrow-left',
    size: 'small',
  },
  acceptProps: {
    label: 'Descartar',
    icon: 'pi pi-trash',
    size: 'small',
    severity: 'danger',
  },
}

/**
 * Configuração padrão para um diálogo de confirmação de ACESSAR ITEM SEMELHANTE.
 */
export const ACCESS_ITEM_CONFIRMATION: ConfirmationOptions = {
  message: `Você possui alterações não salvas neste item. Se prosseguir, elas serão perdidas.`,
  header: 'Descartar Alterações?',
  icon: 'pi pi-exclamation-triangle',
  rejectProps: {
    label: 'Continuar Editando',
    severity: 'secondary',
    text: true,
    icon: 'pi pi-arrow-left',
    size: 'small',
  },
  acceptProps: {
    label: 'Descartar e Abrir',
    icon: 'pi pi-trash',
    size: 'small',
    severity: 'danger',
  },
}

/**
 * Configuração padrão para um diálogo de confirmação de descartar alterações numa página.
 */
export const DISCARD_SOLICITATION_CONFIRMATION: ConfirmationOptions = {
  message: 'Você possui alterações não salvas. Deseja continuar?',
  header: 'Descartar Solicitação?',
  icon: 'pi pi-exclamation-triangle',
  rejectProps: {
    label: 'Continuar Solicitação',
    severity: 'secondary',
    text: true,
    icon: 'pi pi-arrow-left',
    size: 'small',
  },
  acceptProps: {
    label: 'Descartar',
    icon: 'pi pi-trash',
    size: 'small',
    severity: 'danger',
  },
}

/**
 * Configuração padrão para um diálogo de confirmação ao tentar criar outro tipo de solicitação
 * enquanto uma já está em andamento.
 */
export const CHANGE_SOLICITATION_CONFIRMATION: ConfirmationOptions = {
  header: 'Solicitação Existente',
  icon: 'pi pi-exclamation-triangle',
  rejectProps: {
    label: 'Cancelar',
    severity: 'secondary',
    text: true,
    icon: 'pi pi-arrow-left',
    size: 'small',
  },
  acceptProps: {
    label: 'Descartar e Iniciar Nova',
    icon: 'pi pi-trash',
    size: 'small',
    severity: 'danger',
  },
}
