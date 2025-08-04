import { onMounted, onUnmounted, type Ref } from 'vue'

/**
 * Um composable que pede confirmação ao usuário antes de sair da página
 * se houver alterações não salvas.
 * @param isDirty Uma referência reativa (ref ou computed) que é 'true' se houver alterações.
 */
export function useLeaveConfirmation(isDirty: Ref<boolean>) {
  
  // Lida com F5, fechar aba, etc. (Este AINDA precisa usar a API do navegador)
  const handleBeforeUnload = (event: BeforeUnloadEvent) => {
    if (isDirty.value) {
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
