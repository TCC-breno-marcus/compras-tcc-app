<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { useSettingStore } from '../stores/settingStore'
import { InputText, Button, Card } from 'primevue'
import type { Setting } from '../types'
import { useSettingsForm } from '@/composables/useSettingsForm'

const settingsStore = useSettingStore()
const { settings } = storeToRefs(settingsStore)

const {
  isEditing,
  isLoading,
  formData,
  isDirty,
  prazoSubmissaoDatePicker,
  handleSave,
  handleCancel,
} = useSettingsForm(settings)

onMounted(() => {
  if (!settings.value) {
    settingsStore.fetchSettings()
  }
})
</script>

<!-- TODO: ajustar a logica pra replicar igual o SolicitationsSettings -->
<template>
  <div class="flex flex-column w-full gap-2">
    <div class="flex flex-column gap-3">
      <Card>
        <template #title>
          <div class="flex align-items-center gap-2">
            <i class="pi pi-bell text-xl"></i>
            <span>Configurações de Notificações</span>
          </div>
        </template>
        <template #content>
          <ul class="list-none p-0 pt-2 m-0">
            <li
              class="flex flex-column sm:flex-row sm:align-items-center justify-content-between mb-2"
            >
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
              <strong v-else class="text-base">{{ formData.emailParaNotificacoes }}</strong>
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
        <Button
          label="Salvar Alterações"
          icon="pi pi-check"
          size="small"
          @click="handleSave"
          :disabled="!isDirty"
        />
      </div>
    </div>
  </div>
</template>

<style scoped></style>
