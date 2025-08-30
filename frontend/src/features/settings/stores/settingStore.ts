import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { settingService } from '@/features/settings/services/settingService'
import type { Setting } from '../types'

export const useSettingStore = defineStore('setting', () => {
  const settings = ref<Setting | null>(null)
  const settingsBackup = ref<Setting | null>(null)
  const isLoading = ref(false)

  const deadline = computed(() => settings.value?.prazoSubmissao || null)

  /**
   * Busca TODAS as configurações da API.
   */
  const fetchSettings = async () => {
    if (settings.value) {
      return
    }

    isLoading.value = true
    try {
      const response = await settingService.getSettings()
      if (response.prazoSubmissao) {
        response.prazoSubmissao = new Date(response.prazoSubmissao)
      }

      settings.value = response
      settingsBackup.value = JSON.parse(JSON.stringify(response))
    } catch (error) {
      console.error('Erro ao buscar configurações:', error)
    } finally {
      isLoading.value = false
    }
  }

  return { settings, settingsBackup, isLoading, deadline, fetchSettings }
})
