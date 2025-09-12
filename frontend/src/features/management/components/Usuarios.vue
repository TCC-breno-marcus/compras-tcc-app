<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import { storeToRefs } from 'pinia'
import {
  Button,
  ButtonGroup,
  Card,
  FloatLabel,
  IconField,
  InputIcon,
  InputText,
  Tag,
  useConfirm,
  useToast,
} from 'primevue'
import Avatar from 'primevue/avatar'
import Select from 'primevue/select'
import Divider from 'primevue/divider'
import { useUserStore } from '@/features/users/stores/userStore'
import { toTitleCase } from '@/utils/stringUtils'
import UserListSkeleton from '@/features/users/components/UserListSkeleton.vue'
import {
  SAVE_CONFIRMATION,
  UPDATE_STATUS_USER_CONFIRMATION,
} from '@/utils/confirmationFactoryUtils'
import type { User } from '@/features/users/types'
import { roleService } from '@/features/users/services/roleService'
import UserRegister from '@/features/users/components/UserRegister.vue'
import { userService } from '@/features/users/services/userService'

const userStore = useUserStore()
const { users, isLoading: usersLoading, activeUsers, inactiveUsers } = storeToRefs(userStore)
const confirm = useConfirm()
const toast = useToast()
const editingUserId = ref<number | null>(null)
const roleOptions = ref(['Admin', 'Gestor', 'Solicitante'])
const tempRole = ref<'Admin' | 'Gestor' | 'Solicitante'>('Solicitante')
const isCreateDialogVisible = ref(false)
const isActiveUserFilter = ref<boolean>(true)
const searchTermFilter = ref<string>('')

const listUsers = computed(() => {
  const baseUsers = isActiveUserFilter.value ? activeUsers.value : inactiveUsers.value

  if (!searchTermFilter.value.trim()) {
    return baseUsers
  }

  const searchTerm = searchTermFilter.value.toLowerCase().trim()

  return baseUsers.filter(
    (user) =>
      user.nome?.toLowerCase().includes(searchTerm) ||
      user.email?.toLowerCase().includes(searchTerm) ||
      user.departamento?.toLowerCase().includes(searchTerm) ||
      user.cpf?.includes(searchTerm),
  )
})

const getRoleTagSeverity = (role: string) => {
  if (role === 'Admin') return 'danger'
  if (role === 'Gestor') return 'info'
  return 'secondary'
}

const startEditing = (user: User) => {
  editingUserId.value = user.id
  tempRole.value = user.role
}

const acceptSaveChanges = async (user: User) => {
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
    handleCancelEditRole(user)
  }

  editingUserId.value = null
}

const handleCancelEditRole = (user: User) => {
  user.role = tempRole.value
  editingUserId.value = null
}

const handleRoleChange = (user: User) => {
  confirm.require({
    ...SAVE_CONFIRMATION,
    accept: async () => acceptSaveChanges(user),
  })
}

const toggleUserStatus = (user: User) => {
  confirm.require({
    ...UPDATE_STATUS_USER_CONFIRMATION,
    message: `Tem certeza que deseja ${user.isActive ? 'DESATIVAR' : 'ATIVAR'} o usuário ${user.nome}?`,
    accept: async () => {
      if (user.isActive) {
        await userService.inactiveUser(user.id)
      } else {
        await userService.activeUser(user.id)
      }
      updateUserList()
    },
  })
}

const updateUserList = () => {
  userStore.fetchUsers()
}

watch(
  () => userStore.users.length,
  (newLength) => {
    if (newLength === 0) {
      updateUserList()
    }
  },
  { immediate: true },
)
</script>

<template>
  <div class="flex flex-column w-full gap-2">
    <div class="flex flex-column gap-3">
      <Card>
        <template #content>
          <div class="flex align-items-center justify-content-between mb-4">
            <div class="flex gap-2">
              <i class="pi pi-users text-primary text-2xl"></i>
              <div>
                <span class="font-semibold">Gerenciar Usuários e Perfis</span>
                <p class="text-sm text-gray-500 mt-1 mb-0">
                  Visualizar todos os usuários do sistema e trocar seus perfis e status.
                </p>
              </div>
            </div>
          </div>
          <UserListSkeleton v-if="usersLoading" />
          <div v-else>
            <div class="flex align-items-start justify-content-between">
              <div class="flex align-items-start gap-2 pl-4 mb-2">
                <FloatLabel class="w-full sm:w-18rem" variant="on">
                  <IconField iconPosition="left">
                    <InputIcon class="pi pi-search"></InputIcon>
                    <InputText
                      v-model="searchTermFilter"
                      size="small"
                      class="w-full"
                      inputId="simple-search"
                    />
                    <InputIcon
                      v-if="searchTermFilter"
                      class="pi pi-times cursor-pointer"
                      @click="searchTermFilter = ''"
                    />
                  </IconField>
                  <label for="simple-search">Buscar usuário</label>
                </FloatLabel>
                <ButtonGroup>
                  <Button
                    type="button"
                    label="Ativos"
                    icon="pi pi-check-circle"
                    size="small"
                    :outlined="!isActiveUserFilter"
                    @click="isActiveUserFilter = true"
                  />
                  <Button
                    type="button"
                    label="Inativos"
                    icon="pi pi-times-circle"
                    size="small"
                    :outlined="isActiveUserFilter"
                    @click="isActiveUserFilter = false"
                  />
                </ButtonGroup>
              </div>
              <Button
                type="button"
                label="Novo Usuário"
                icon="pi pi-plus"
                size="small"
                text
                @click="isCreateDialogVisible = true"
              />
            </div>

            <ul class="list-none p-4 pt-2 m-0 h-25rem overflow-x-auto">
              <template v-for="(user, index) in listUsers" :key="user.id">
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

                        <Button
                          icon="pi pi-power-off"
                          :severity="user.isActive ? 'danger' : 'success'"
                          text
                          rounded
                          class="edit-button"
                          v-tooltip.top="user.isActive ? 'Desativar Usuário' : 'Ativar Usuário'"
                          @click="toggleUserStatus(user)"
                          :disabled="editingUserId !== null"
                        />
                      </template>
                    </div>
                  </div>
                </li>
                <Divider v-if="index < activeUsers.length - 1" />
              </template>
              <div v-if="listUsers.length === 0">
                <h4 class="mb-1">Nenhum resultado encontrado.</h4>
                <p>Tente ajustar os filtros ou utilize termos de busca diferentes.</p>
              </div>
            </ul>
          </div>
        </template>
      </Card>
    </div>
  </div>

  <UserRegister
    v-model:visible="isCreateDialogVisible"
    @update:visible="isCreateDialogVisible = false"
    @update:user-list="updateUserList"
  />
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
