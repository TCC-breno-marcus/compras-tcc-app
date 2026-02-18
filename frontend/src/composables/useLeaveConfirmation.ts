import { computed, onMounted, onUnmounted, type Ref } from 'vue'
import { onBeforeRouteLeave } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import { useSolicitationCartStore } from '@/features/solicitations/stores/solicitationCartStore'
import { storeToRefs } from 'pinia'
import { DISCARD_SOLICITATION_CONFIRMATION } from '@/utils/confirmationFactoryUtils'

/**
 * Um composable que pede confirmação ao usuário antes de sair da página
 * se houver uma solicitação em andamento.
 * Lida tanto com a navegação interna (Vue Router) quanto externa (fechar aba/refresh).
 */
export const useLeaveConfirmation = () => {
  const confirm = useConfirm()
  const solicitationCartStore = useSolicitationCartStore()
  const { solicitationItems, justification } = storeToRefs(solicitationCartStore)

  const hasUnsavedChanges = computed(
    () => solicitationItems.value.length > 0 || justification.value.trim() !== '',
  )

  // NAVEGAÇÃO INTERNA (Vue Router)
  onBeforeRouteLeave((to, from, next) => {
    const isNavigatingToAnotherCreatePage = to.meta.isCreateSolicitationPage === true

    // Só mostra o diálogo se houver itens E se estiver navegando para outra página de criação
    if (hasUnsavedChanges.value && isNavigatingToAnotherCreatePage) {
      confirm.require({
        ...DISCARD_SOLICITATION_CONFIRMATION,
        accept: () => {
          solicitationCartStore.$reset()
          next()
        },
        reject: () => {
          next(false)
        },
      })
    } else {
      next()
    }
  })

  // Lida com F5, fechar aba
  /**
   * Intercepta saída da página fora do Vue Router quando há alterações pendentes.
   */
  const handleBeforeUnload = (event: BeforeUnloadEvent) => {
    if (hasUnsavedChanges.value) {
      event.preventDefault()
      event.returnValue = 'Você tem alterações não salvas. Deseja mesmo sair?'
    }
  }

  onMounted(() => {
    window.addEventListener('beforeunload', handleBeforeUnload)
  })

  onUnmounted(() => {
    window.removeEventListener('beforeunload', handleBeforeUnload)
  })
}
