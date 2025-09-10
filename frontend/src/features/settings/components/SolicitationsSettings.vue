<script setup lang="ts">
import { onMounted } from 'vue'
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

<template>
  <div class="min-h-screen">
    <div class="max-w-4xl mx-auto">
      <Card class="shadow-lg border-none">
        <!-- <template #header>
          <div class="flex align-items-center bg-primary text-white gap-2 p-4 border-round-top-lg">
            <div class="p-2 border-radius-lg">
              <i class="pi pi-cog text-2xl"></i>
            </div>
            <div>
              <h2 class="text-xl font-semibold m-0">Regras de Solicitação</h2>
              <p class="text-secondary mt-1 mb-0 text-sm">
                Configure os limites e prazos do sistema
              </p>
            </div>
          </div>
        </template> -->

        <template #content>
          <div v-if="isLoading" class="p-4">
            <div class="flex flex-column gap-4">
              <div v-for="i in 3" :key="i" class="flex justify-content-between align-items-center">
                <Skeleton width="60%" height="1.2rem" />
                <Skeleton width="8rem" height="2.5rem" />
              </div>
            </div>
          </div>

          <div v-else class="flex flex-column gap-2">
            <!-- Setting Item 1 -->
            <div class="flex justify-content-between">
              <div class="">
                <div class="flex align-items-center gap-2">
                  <i class="pi pi-calendar text-primary"></i>
                  <div>
                    <span class="font-semibold text-gray-900">Prazo final para criação/edição</span>
                    <p class="text-sm text-gray-500 mt-1 mb-0">
                      Define até quando solicitações podem ser criadas ou modificadas
                    </p>
                  </div>
                </div>
              </div>

              <div>
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
                  placeholder="Selecione a data"
                />
                <div v-else class="value-display">
                  <span class="value-text">
                    {{ formatDate(formData.prazoSubmissao || 'Não cadastrado') }}
                  </span>
                </div>
              </div>
            </div>

            <Divider />

            <!-- Setting Item 2 -->

            <div class="flex justify-content-between">
              <div class="">
                <div class="flex align-items-center gap-2">
                  <i class="pi pi-box text-primary"></i>
                  <div>
                    <span class="font-semibold text-gray-900">Quantidade máxima por item</span>
                    <p class="text-sm text-gray-500 mt-1 mb-0">
                      Número máximo de unidades que podem ser solicitadas por item
                    </p>
                  </div>
                </div>
              </div>

              <div>
                <InputNumber
                  v-if="isEditing"
                  v-model="formData.maxQuantidadePorItem"
                  inputId="max-qtd-itens-input"
                  :min="1"
                  size="small"
                  inputClass="w-full sm:w-10rem"
                />
                <div v-else class="value-display">
                  <span class="value-text">{{ formData.maxQuantidadePorItem }}</span>
                  <span class="value-unit">unidades/item</span>
                </div>
              </div>
            </div>

            <Divider />

            <!-- Setting Item 3 -->
            <div class="flex justify-content-between">
              <div class="">
                <div class="flex align-items-center gap-2">
                  <i class="pi pi-list text-primary"></i>
                  <div>
                    <span class="font-semibold text-gray-900"
                      >Itens diferentes por solicitação</span
                    >
                    <p class="text-sm text-gray-500 mt-1 mb-0">
                      Número máximo de tipos de itens diferentes em uma solicitação
                    </p>
                  </div>
                </div>
              </div>

              <div>
                <InputNumber
                  v-if="isEditing"
                  v-model="formData.maxItensDiferentesPorSolicitacao"
                  inputId="max-itens-input"
                  :min="1"
                  size="small"
                  inputClass="w-full sm:w-10rem"
                />
                <div v-else class="value-display">
                  <span class="value-text">{{ formData.maxItensDiferentesPorSolicitacao }}</span>
                  <span class="value-unit">itens/solicitação</span>
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
                @click="handleCancel()"
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
                  <span>Alterações nas configurações entram em vigor imediatamente</span>
                </li>
                <li class="flex align-items-start gap-2">
                  <i
                    class="pi pi-circle-fill text-xs text-gray-400 flex-shrink-0"
                    style="margin-top: 0.4rem"
                  ></i>
                  <span>Solicitações já criadas não são afetadas pelas mudanças</span>
                </li>
                <!-- <li class="flex align-items-start gap-2">
                  <i
                    class="pi pi-circle-fill text-xs text-gray-400 flex-shrink-0"
                    style="margin-top: 0.4rem"
                  ></i>
                  <span>Todas as alterações são registradas no log do sistema</span>
                </li> -->
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
  min-width: 8rem;
  justify-content: center;
}

.value-text {
  font-size: 1.1rem;
  font-weight: 600;
  color: #1e293b;
}

.value-unit {
  font-size: 0.875rem;
  color: #64748b;
  font-weight: 500;
}

/* Responsive */
@media (min-width: 768px) {
  .setting-content {
    flex-direction: row;
    align-items: flex-start;
    justify-content: space-between;
  }
}
</style>
