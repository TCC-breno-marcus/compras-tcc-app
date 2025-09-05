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
      <Card class="shadow-lg border-none">
        <template #content>
          <div v-if="isLoading" class="p-4">
            <div class="flex flex-column gap-4">
              <div class="flex justify-content-between align-items-center">
                <Skeleton width="60%" height="1.2rem" />
                <Skeleton width="4rem" height="2rem" />
              </div>
            </div>
          </div>

          <div v-else class="flex flex-column gap-2">
            <!-- User Registration Setting -->
            <div class="flex justify-content-between">
              <div>
                <div class="flex align-items-center gap-2">
                  <i class="pi pi-user-plus text-primary"></i>
                  <div>
                    <span class="font-semibold text-gray-900"
                      >Permitir Auto-Cadastro de Novos Usuários</span
                    >
                    <p class="text-sm text-gray-500 mt-1 mb-0">
                      Permite que novos usuários se cadastrem automaticamente no sistema
                    </p>
                  </div>
                </div>
              </div>

              <div>
                <div v-if="isEditing" class="flex align-items-center">
                  <ToggleSwitch
                    v-model="formData.permitirAutoCadastro"
                    inputId="auto-cadastro-switch"
                    class="toggle-switch"
                  />
                </div>
                <div v-else class="value-display-toggle">
                  <div class="flex align-items-center gap-2">
                    <i
                      :class="
                        formData.permitirAutoCadastro
                          ? 'pi pi-check-circle text-green-500'
                          : 'pi pi-times-circle text-red-500'
                      "
                    ></i>
                    <span class="font-semibold">
                      {{ formData.permitirAutoCadastro ? 'Habilitado' : 'Desabilitado' }}
                    </span>
                  </div>
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

      <Card>
        <!-- <template #title>
          <div class="flex align-items-center gap-2">
            <i class="pi pi-users text-xl"></i>
            <span>Gerenciar Usuários e Perfis</span>
          </div>
        </template> -->
        <template #content>
          <div class="flex align-items-center gap-2 mb-4">
            <i class="pi pi-sliders-h text-primary"></i>
            <div>
              <span class="font-semibold text-gray-900">Gerenciar Usuários e Perfis</span>
              <p class="text-sm text-gray-500 mt-1 mb-0">
                Visualizar todos os usuários do sistema e trocar seus perfis.
              </p>
            </div>
          </div>
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

.value-display-toggle {
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

.toggle-switch {
  transform: scale(1.2);
}
</style>
