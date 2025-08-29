import { ref } from 'vue'
import { defineStore } from 'pinia'
import { settingService } from '@/features/settings/services/settingService'

export const useSettingStore = defineStore('setting', () => {
  const deadline = ref<Date | null>(null)
  const isLoading = ref(false)

  /**
   * Busca o prazo da API, mas apenas se ele ainda não foi carregado.
   */
  const fetchPrazoSubmissao = async () => {
    if (deadline.value) {
      return
    }

    isLoading.value = true
    try {
      const response = await settingService.getPrazoSubmissao()
      deadline.value = new Date(response.prazoSubmissao)
    } catch (error) {
      console.error(error)
    } finally {
      isLoading.value = false
    }
  }

  return { deadline, isLoading, fetchPrazoSubmissao }
})
