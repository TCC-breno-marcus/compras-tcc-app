<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { useSettingStore } from '../stores/settingStore'
import { Button, Card, ToggleSwitch } from 'primevue'
import { useSettingsForm } from '@/composables/useSettingsForm'
import Avatar from 'primevue/avatar'
import Select from 'primevue/select'
import Divider from 'primevue/divider'

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

const users = ref([
  { id: 1, nome: 'Ana Carolina Souza', email: 'ana.souza@sistema.com', role: 'Admin' },
  { id: 2, nome: 'Bruno Lima', email: 'bruno.lima@sistema.com', role: 'Gestor' },
  { id: 3, nome: 'Carlos Pereira', email: 'carlos.p@sistema.com', role: 'Solicitante' },
  { id: 4, nome: 'Daniela Martins', email: 'daniela.m@sistema.com', role: 'Solicitante' },
  { id: 5, nome: 'Eduardo Costa', email: 'eduardo.c@sistema.com', role: 'Solicitante' },
])

const roleOptions = ref(['Admin', 'Gestor', 'Solicitante'])
</script>

<template>
  <div class="flex flex-column w-full gap-2">
    <div class="flex flex-column gap-3">
      <Card>
        <template #title>
          <div class="flex align-items-center gap-2">
            <i class="pi pi-user-plus text-xl"></i>
            <span>Criação de Novos Usuários</span>
          </div>
        </template>
        <template #content>
          <ul class="list-none p-0 pt-2 m-0">
            <li
              class="flex flex-column sm:flex-row sm:align-items-center justify-content-between mb-2"
            >
              <label for="auto-cadastro-switch" class="text-color-secondary">
                Permitir Auto-Cadastro de Novos Usuários
              </label>
              <ToggleSwitch
                v-model="formData.permitirAutoCadastro"
                inputId="auto-cadastro-switch"
                :disabled="!isEditing"
              />
            </li>
          </ul>
        </template>
      </Card>

      <Card>
        <template #title>
          <div class="flex align-items-center gap-2">
            <i class="pi pi-users text-xl"></i>
            <span>Gerenciar Usuários e Perfis</span>
          </div>
        </template>
        <template #content>
          <ul class="list-none p-4 pt-2 m-0 h-25rem overflow-x-auto">
            <template v-for="(user, index) in users" :key="user.id">
              <li class="">
                <div class="flex align-items-center justify-content-between w-full">
                  <div class="flex align-items-center gap-3">
                    <Avatar :label="user.nome.charAt(0)" shape="circle" size="small" />
                    <div>
                      <p class="font-bold m-0">{{ user.nome }}</p>
                      <p class="text-sm text-color-secondary m-0">{{ user.email }}</p>
                    </div>
                  </div>

                  <div>
                    <Select
                      v-if="isEditing"
                      v-model="user.role"
                      :options="roleOptions"
                      class="w-full sm:w-9rem"
                      size="small"
                    />
                    <span v-else class="font-medium text-color-secondary">{{ user.role }}</span>
                  </div>
                </div>
              </li>
              <Divider v-if="index < users.length - 1" />
            </template>
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
