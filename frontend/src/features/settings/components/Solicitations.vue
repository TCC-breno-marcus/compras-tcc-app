<script setup lang="ts">
import { ref, onMounted, inject, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { useSettingStore } from '../stores/settingStore'
import { DatePicker, FloatLabel, InputNumber } from 'primevue'
import { SettingsContextKey } from '../settings.keys'
import { formatDate } from '@/utils/dateUtils'
import type { Setting } from '../types'

const settingsContext = inject(SettingsContextKey)

const settingsStore = useSettingStore()
// 'settings' é o estado atual, 'settingsBackup' é a cópia para o 'cancelar'
const { settings, settingsBackup, isLoading } = storeToRefs(settingsStore)

const isEditing = ref(false)

const formData = ref<Setting>({
  maxItensSolicitacao: 0,
  prazoSubmissao: null,
})

onMounted(() => {
  settingsStore.fetchSettings()
})

const handleSave = async () => {
  // if (!isSettingsValid(settings.value)) return; // Validação

  // await settingsStore.updateSettings(settings.value)
  isEditing.value = false
}

const handleCancel = () => {
  settingsStore.$reset() // Reseta para o valor do backup
  isEditing.value = false
}

watch(
  settings,
  (newSettings) => {
    if (newSettings) {
      formData.value = { ...newSettings } // Cria uma cópia
    }
    console.log(formData)
  },
  { immediate: true, deep: true },
)
</script>

<template>
  <div class="flex w-full">
    <div class="mt-4 surface-card p-4 border-round">
      <div v-if="settingsContext?.isEditing" class="flex flex-column gap-4">
        <FloatLabel>
          <DatePicker
            v-model="formData.prazoSubmissao"
            inputId="prazo-picker"
            class="w-full md:w-14rem"
          />
          <label for="prazo-picker">Prazo final para edição</label>
        </FloatLabel>
        <FloatLabel>
          <InputNumber v-model="formData.maxItensSolicitacao" inputId="max-itens-input" />
          <label for="max-itens-input">Nº Máximo de Itens por Solicitação</label>
        </FloatLabel>
      </div>

      <div v-else class="flex flex-column gap-3">
        <div>
          <label class="block text-sm text-color-secondary">Prazo final para edição</label>
          <p class="font-medium mt-1">{{ formatDate(formData.prazoSubmissao!) }}a</p>
        </div>
        <div>
          <label class="block text-sm text-color-secondary"
            >Nº Máximo de Itens por Solicitação</label
          >
          <p class="font-medium mt-1">{{ formData.maxItensSolicitacao }}a</p>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped></style>
