<script setup lang="ts">
import { onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { useSettingStore } from '../stores/settingStore'
import { InputText, Button, Card } from 'primevue'
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

<template>
  <div class="flex flex-column w-full gap-2">
    <div class="flex flex-column gap-3">
      <Card class="shadow-lg border-none">
        <template #content>
          <div v-if="isLoading" class="p-4">
            <div class="flex flex-column gap-4">
              <div class="flex justify-content-between align-items-center">
                <Skeleton width="60%" height="1.2rem" />
                <Skeleton width="12rem" height="2.5rem" />
              </div>
            </div>
          </div>

          <div v-else class="flex flex-column gap-2">
            <!-- Email Setting -->
            <div class="flex justify-content-between">
              <div>
                <div class="flex align-items-center gap-2">
                  <i class="pi pi-envelope text-primary"></i>
                  <div>
                    <span class="font-semibold">Email de Notificações</span>
                    <p class="text-sm text-gray-500 mt-1 mb-0">
                      Email usado para envio de notificações
                    </p>
                  </div>
                </div>
              </div>

              <div>
                <InputText
                  v-if="isEditing"
                  name="email"
                  v-model="formData.emailParaNotificacoes"
                  type="email"
                  class="w-full sm:w-16rem"
                  size="small"
                  :invalid="!formData.emailParaNotificacoes"
                  placeholder="exemplo@empresa.com"
                />
                <div v-else class="value-display">
                  <span>
                    {{ formData.emailParaNotificacoes }}
                  </span>
                </div>
              </div>
            </div>
          </div>
        </template>

        <template #footer>
          <div class="flex justify-content-end gap-3 p-3">
            <Button
              v-if="!isEditing"
              label="Editar"
              icon="pi pi-pencil"
              size="small"
              @click="isEditing = true"
            />

            <template v-else>
              <Button
                label="Cancelar"
                icon="pi pi-times"
                severity="secondary"
                @click="handleCancel"
                size="small"
              />
              <Button
                label="Salvar Alterações"
                icon="pi pi-check"
                :disabled="!isDirty"
                :loading="isLoading"
                @click="handleSave"
                size="small"
              />
            </template>
          </div>
        </template>
      </Card>

      <!-- Card de informações adicionais -->
      <Card class="mt-4 shadow-sm">
        <template #content>
          <div class="flex align-items-start gap-3 p-2">
            <i class="pi pi-info-circle text-blue-500 text-lg flex-shrink-0 mt-1"></i>
            <div class="text-sm text-gray-600 line-height-3">
              <p class="font-medium text-gray-800 mb-2">Informações importantes:</p>
              <ul class="list-none p-0 m-0 flex flex-column gap-1">
                <li class="flex align-items-start gap-2">
                  <i
                    class="pi pi-circle-fill text-xs text-gray-400 flex-shrink-0"
                    style="margin-top: 0.4rem"
                  ></i>
                  <span>Certifique-se de que o email esteja válido e ativo</span>
                </li>
                <li class="flex align-items-start gap-2">
                  <i
                    class="pi pi-circle-fill text-xs text-gray-400 flex-shrink-0"
                    style="margin-top: 0.4rem"
                  ></i>
                  <span>Alterações entram em vigor imediatamente</span>
                </li>
              </ul>
            </div>
          </div>
        </template>
      </Card>
    </div>
  </div>
</template>

<style scoped>
.value-display {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.4rem 1rem;
  background: var(--p-surface-50);
  border-radius: 8px;
  border: 1px solid #e2e8f0;
  min-width: 12rem;
  justify-content: center;
}
</style>
