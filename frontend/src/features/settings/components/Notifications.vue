<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { useSettingStore } from '../stores/settingStore'
import { InputText, Button, Card } from 'primevue'
import type { Setting } from '../types'

const settingsStore = useSettingStore()
const { settings, settingsBackup, isLoading } = storeToRefs(settingsStore)

const isEditing = ref(false)

const formData = ref<Partial<Setting>>({
  maxQuantidadePorItem: 0,
  maxItensDiferentesPorSolicitacao: 0,
})

onMounted(() => {
  if (!settings.value) {
    settingsStore.fetchSettings()
  }
})

const handleSave = async () => {
  // if (!isSettingsValid(settings.value)) return; // Validação

  // await settingsStore.updateSettings(settings.value)
  isEditing.value = false
}

const handleCancel = () => {
  isEditing.value = false
  // settingsStore.$reset() // Reseta para o valor do backup
}

watch(
  settings,
  (newSettings) => {
    if (newSettings) {
      formData.value = { ...newSettings }
    }
    console.log(formData)
  },
  { immediate: true, deep: true },
)
</script>

<template>
  <div class="flex flex-column w-full gap-2">
    <div class="flex flex-column gap-3">
      <Card>
        <template #title>
          <div class="flex align-items-center gap-2">
            <i class="pi pi-envelope text-xl"></i>
            <span>Contatos e Notificações</span>
          </div>
        </template>
        <template #content>
          <ul class="list-none p-0 m-0">
            <li class="flex align-items-center justify-content-between mb-2">
              <span class="text-color-secondary">Email de Contato Principal</span>
              <InputText
                v-if="isEditing"
                name="email"
                v-model="formData.emailContatoPrincipal"
                type="email"
                class="w-full sm:w-16rem"
                size="small"
                :invalid="!formData.emailContatoPrincipal"
              />
              <strong v-else class="text-lg">{{ formData.emailContatoPrincipal }}</strong>
            </li>
            <li class="flex align-items-center justify-content-between">
              <span class="text-color-secondary">Email para envio de notificações</span>
              <InputText
                v-if="isEditing"
                name="email"
                v-model="formData.emailParaNotificacoes"
                type="email"
                class="w-full sm:w-16rem"
                size="small"
                :invalid="!formData.emailParaNotificacoes"
              />
              <strong v-else class="text-lg">{{ formData.emailParaNotificacoes }}</strong>
            </li>
          </ul>
        </template>
      </Card>
    </div>

    <div class="flex w-full justify-content-end">
      <Button
        v-if="!isEditing"
        label="Editar"
        icon="pi pi-pencil"
        size="small"
        @click="isEditing = true"
      />
      <div v-else class="flex gap-2">
        <Button label="Cancelar" severity="secondary" size="small" @click="handleCancel" text />
        <Button label="Salvar Alterações" icon="pi pi-check" size="small" @click="handleSave" />
      </div>
    </div>
  </div>
</template>

<style scoped></style>
