<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { useSettingStore } from '../stores/settingStore'
import { Button, Card, ToggleSwitch, Tag, useConfirm, useToast } from 'primevue'
import { useSettingsForm } from '@/composables/useSettingsForm'
import Avatar from 'primevue/avatar'
import Select from 'primevue/select'
import Divider from 'primevue/divider'
import { useUserStore } from '@/features/users/stores/userStore'
import { toTitleCase } from '@/utils/stringUtils'
import UserListSkeleton from './UserListSkeleton.vue'
import { SAVE_CONFIRMATION } from '@/utils/confirmationFactoryUtils'
import type { User } from '@/features/users/types'
import { roleService } from '@/features/users/services/roleService'

const settingsStore = useSettingStore()
const { settings } = storeToRefs(settingsStore)
const userStore = useUserStore()
const { users, isLoading: usersLoading } = storeToRefs(userStore)
const confirm = useConfirm()
const toast = useToast()

const editingUserId = ref<number | null>(null)

const {
  isEditing,
  isLoading,
  formData,
  isDirty,
  prazoSubmissaoDatePicker,
  handleSave,
  handleCancel,
} = useSettingsForm(settings)

const roleOptions = ref(['Admin', 'Gestor', 'Solicitante'])

const getRoleTagSeverity = (role: string) => {
  if (role === 'Admin') return 'danger'
  if (role === 'Gestor') return 'info'
  return 'secondary'
}

const tempRole = ref<string>('')

const startEditing = (user: any) => {
  editingUserId.value = user.id
  tempRole.value = user.role
}

const acceptSaveChanges = async (user: User) => {
  console.log(`Perfil do usuário ${user.nome} alterado para ${user.role}. Salvando...`)

  try {
    await roleService.updateUserRole({
      email: user.email,
      role: user.role as 'Admin' | 'Gestor' | 'Solicitante',
    })
    toast.add({
      severity: 'success',
      summary: 'Sucesso',
      detail: `Perfil do usuário ${user.nome} alterado para ${user.role}.`,
      life: 3000,
    })
  } catch (err) {
    console.error('Erro ao salvar as alterações:', err)
    toast.add({
      severity: 'error',
      summary: 'Erro',
      detail: 'Não foi possível salvar as alterações.',
      life: 3000,
    })
  }

  editingUserId.value = null
}

const handleCancelEditRole = (user: any) => {
  user.role = tempRole.value
  editingUserId.value = null
}

const handleRoleChange = (user: User) => {
  confirm.require({
    ...SAVE_CONFIRMATION,
    accept: async () => acceptSaveChanges(user),
  })
}

onMounted(() => {
  if (!settings.value) {
    settingsStore.fetchSettings()
  }
  userStore.fetchUsers()
})
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
      <Card>
        <template #title>
          <div class="flex align-items-center gap-2">
            <i class="pi pi-users text-xl"></i>
            <span>Gerenciar Usuários e Perfis</span>
          </div>
        </template>
        <template #content>
          <UserListSkeleton v-if="usersLoading" />
          <ul v-else class="list-none p-4 pt-2 m-0 h-25rem overflow-x-auto">
            <template v-for="(user, index) in users" :key="user.id">
              <li
                class="user-row"
                :class="{ 'opacity-50': editingUserId !== null && editingUserId !== user.id }"
              >
                <div class="flex align-items-center justify-content-between w-full">
                  <div class="flex align-items-start gap-3">
                    <Avatar :label="user.nome.charAt(0)" shape="circle" size="small" />
                    <div class="flex flex-column gap-">
                      <p class="font-bold m-0">{{ user.nome }}</p>
                      <span class="text-color-secondary">{{ user.email }}</span>
                      <span class="text-color-secondary">{{
                        user.departamento === 'não disponível'
                          ? 'N/A'
                          : toTitleCase(user.departamento)
                      }}</span>
                    </div>
                  </div>

                  <div class="flex align-items-center gap-2">
                    <template v-if="editingUserId === user.id">
                      <Select
                        v-if="editingUserId === user.id"
                        v-model="user.role"
                        :options="roleOptions"
                        class="w-full sm:w-9rem"
                        size="small"
                      />
                      <Button
                        icon="pi pi-times"
                        severity="danger"
                        text
                        rounded
                        @click="handleCancelEditRole(user)"
                        v-tooltip.top="'Cancelar'"
                        size="small"
                      />
                      <Button
                        icon="pi pi-check"
                        severity="success"
                        text
                        rounded
                        @click="handleRoleChange(user)"
                        v-tooltip.top="'Salvar'"
                        size="small"
                      />
                    </template>
                    <template v-else>
                      <Tag :value="user.role" :severity="getRoleTagSeverity(user.role)" />
                      <Button
                        icon="pi pi-pencil"
                        text
                        rounded
                        class="edit-button"
                        size="small"
                        severity="secondary"
                        @click="startEditing(user)"
                        v-tooltip="'Trocar Perfil'"
                        :disabled="editingUserId !== null"
                      />
                    </template>
                  </div>
                </div>
              </li>
              <Divider v-if="index < users.length - 1" />
            </template>
          </ul>
        </template>
      </Card>
    </div>
  </div>
</template>

<style scoped>
.edit-button {
  transition: opacity 0.2s ease-in-out;
}

.user-row:not(.opacity-50) .edit-button {
  opacity: 0;
}
.user-row:not(.opacity-50):hover .edit-button {
  opacity: 1;
}
</style>
