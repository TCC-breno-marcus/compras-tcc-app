<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue'
import { storeToRefs } from 'pinia'
import { useSettingStore } from '../stores/settingStore'
import { DatePicker, InputNumber, Button, Card } from 'primevue'
import { formatDate } from '@/utils/dateUtils'
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

<!-- TODO: implementar esses parametros em cada lugar do projeto os deve usar (max de itens e quantidade) -->
<template>
  <div class="flex flex-column w-full gap-2">
    <div class="flex flex-column gap-3">
      <Card class="mb-4">
        <template #title>
          <div class="flex align-items-center gap-2">
            <i class="pi pi-file-edit text-xl"></i>
            <span>Regras Gerais de Solicitação</span>
          </div>
        </template>
        <template #content>
          <ul class="list-none p-0 pt-2 m-0">
            <li
              class="flex flex-column sm:flex-row sm:align-items-center justify-content-between mb-2"
            >
              <span class="text-color-secondary mb-1">Prazo final para criação/edição</span>
              <DatePicker
                v-if="isEditing"
                v-model="prazoSubmissaoDatePicker"
                inputId="prazo-picker"
                dateFormat="dd/mm/yy"
                showIcon
                class="w-full sm:w-10rem"
                iconDisplay="input"
                size="small"
                :invalid="!prazoSubmissaoDatePicker"
              />
              <strong v-else class="text-base">{{
                formatDate(formData.prazoSubmissao || null)
              }}</strong>
            </li>
            <li
              class="flex flex-column sm:flex-row sm:align-items-center justify-content-between mb-2"
            >
              <span class="text-color-secondary mb-1">Nº Máximo de Quantidade por Item</span>
              <InputNumber
                v-if="isEditing"
                v-model="formData.maxQuantidadePorItem"
                inputId="max-qtd-itens-input"
                :min="1"
                size="small"
                inputClass="w-full sm:w-10rem"
              />
              <strong v-else class="text-base">{{ formData.maxQuantidadePorItem }}</strong>
            </li>
            <li class="flex flex-column sm:flex-row sm:align-items-center justify-content-between">
              <span class="text-color-secondary mb-1"
                >Nº Máximo de Itens Diferentes por Solicitação</span
              >
              <InputNumber
                v-if="isEditing"
                v-model="formData.maxItensDiferentesPorSolicitacao"
                inputId="max-itens-input"
                :min="1"
                size="small"
                inputClass="w-full sm:w-10rem"
              />
              <strong v-else class="text-base">{{
                formData.maxItensDiferentesPorSolicitacao
              }}</strong>
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
        <Button label="Salvar Alterações" icon="pi pi-check" size="small" @click="handleSave" :disabled="!isDirty" />
      </div>
    </div>
  </div>
</template>

<style scoped></style>
