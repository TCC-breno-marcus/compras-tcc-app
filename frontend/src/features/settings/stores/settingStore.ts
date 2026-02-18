import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { settingService } from '@/features/settings/services/settingService'
import type { Setting } from '../types'
import { getChangedProperties } from '@/utils/objectUtils'
import { addTimeEndInDate } from '@/utils/dateUtils'

export const useSettingStore = defineStore('setting', () => {
  const settings = ref<Setting | null>(null)
  const isLoading = ref(false)

  const deadline = computed(() => settings.value?.prazoSubmissao || null)

  const deadlineHasExpired = computed(() => {
    if (deadline.value) {
      return new Date() > new Date(deadline.value)
    }
    return false
  })

  /**
   * Busca TODAS as configurações da API.
   */
  const fetchSettings = async () => {
    isLoading.value = true
    try {
      const response = await settingService.getSettings()
      settings.value = response
    } catch (error) {
      console.error('Erro ao buscar configurações:', error)
    } finally {
      isLoading.value = false
    }
  }

  /**
   * Atualiza as configurações.
   * Envia apenas propriedades alteradas e ajusta prazo para fim do dia quando necessário.
   */
  const updateSettings = async (data: Partial<Setting>) => {
    isLoading.value = true
    try {
      const changedProperties = getChangedProperties(settings.value, data)
      const payload = Object.fromEntries(
        changedProperties
          .filter((key) => key in data)
          .map((key) => {
            let value = data[key as keyof typeof data]
            if (key === 'prazoSubmissao' && typeof value === 'string') {
              value = addTimeEndInDate(value)
            }
            return [key, value]
          }),
      )
      const response = await settingService.updateSettings(payload)
      settings.value = response
      return true
    } catch (error) {
      console.error('Erro ao atualizar configurações:', error)
    } finally {
      isLoading.value = false
    }
  }

  return { settings, isLoading, deadline, deadlineHasExpired, fetchSettings, updateSettings }
})
