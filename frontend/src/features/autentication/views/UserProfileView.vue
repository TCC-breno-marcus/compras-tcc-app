<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useAuthStore } from '@/features/autentication/stores/authStore'
import { storeToRefs } from 'pinia'
import { dataHasBeenChanged } from '@/utils/objectUtils'
import Avatar from 'primevue/avatar'
import Button from 'primevue/button'
import Card from 'primevue/card'
import InputMask from 'primevue/inputmask'
import Tag from 'primevue/tag'
import type { User } from '@/features/users/types'
import Message from 'primevue/message'
import { unitOrganizationalFormatString } from '@/utils/organizationalUnit'

const authStore = useAuthStore()
const { user } = storeToRefs(authStore)

const isEditing = ref(false)
const isLoading = ref(false)

const formData = ref<Partial<User>>({})
const formBackup = ref<Partial<User>>({})

const isDirty = computed(() => dataHasBeenChanged(formBackup.value, formData.value))

const getRoleTagSeverity = (role: string) => {
  switch (role) {
    case 'Admin':
      return 'danger'
    case 'Gestor':
      return 'info'
    case 'Solicitante':
      return 'secondary'
    default:
      return 'secondary'
  }
}

onMounted(() => {
  if (user.value) {
    formData.value = { ...user.value }
    formBackup.value = { ...user.value }
  }
})

const handleSave = async () => {
  if (!formData.value.id) return

  isLoading.value = true
  try {
    if (!formData.value.telefone || !formData.value.cpf) {
      throw new Error('Telefone e CPF são obrigatórios')
    }

    // console.log('Salvando dados:', formData.value)

    await new Promise((resolve) => setTimeout(resolve, 1000))

    formBackup.value = { ...formData.value }
    isEditing.value = false
  } catch (error) {
    console.error('Erro ao salvar perfil:', error)
  } finally {
    isLoading.value = false
  }
}

const handleCancel = () => {
  formData.value = { ...formBackup.value }
  isEditing.value = false
}

const changePassword = () => {
  console.log('Abrir modal de alteração de senha')
}
</script>

<!-- TODO: ainda não é possivel editar dados no backend -->
<template>
  <div class="p-2">
    <div class="mb-4">
      <h2 class="text-3xl font-bold m-0 text-color">Meu Perfil</h2>
      <p class="text-color-secondary mt-2 mb-0">
        Gerencie suas informações pessoais e configurações de conta
      </p>
    </div>

    <div class="grid">
      <!-- Profile Avatar Card -->
      <div class="col-12 lg:col-4 xl:col-3">
        <Card class="h-full">
          <template #content>
            <div class="flex flex-column align-items-center text-center gap-3">
              <Avatar
                :label="formData.nome?.charAt(0).toUpperCase()"
                size="xlarge"
                shape="circle"
                class="bg-primary text-0 font-bold text-2xl"
                style="width: 120px; height: 120px"
              />
              <div>
                <h3 class="font-bold m-0 text-xl">{{ formData.nome || 'Carregando...' }}</h3>
                <span class="text-color-secondary text-sm block mt-1">{{ formData.email }}</span>
                <Tag
                  v-if="formData.role"
                  :value="formData.role"
                  :severity="getRoleTagSeverity(formData.role)"
                  class="mt-2"
                />
              </div>
            </div>
          </template>
        </Card>
      </div>

      <!-- Main Content -->
      <div class="col-12 lg:col-8 xl:col-9">
        <!-- Action Buttons -->
        <div class="flex justify-content-end mb-2">
          <Button
            v-if="!isEditing"
            label="Editar Perfil"
            icon="pi pi-pencil"
            severity="secondary"
            outlined
            @click="isEditing = true"
            size="small"
          />
          <div v-else class="flex gap-2">
            <Button label="Cancelar" @click="handleCancel" severity="secondary" text size="small" />
            <Button
              label="Salvar Alterações"
              icon="pi pi-check"
              @click="handleSave"
              :loading="isLoading"
              :disabled="!isDirty"
              size="small"
            />
          </div>
        </div>

        <!-- Personal Information -->
        <Card class="mb-4">
          <template #title>
            <div class="flex align-items-center gap-2">
              <i class="pi pi-user text-primary"></i>
              <span>Informações Pessoais</span>
            </div>
          </template>
          <template #content>
            <div class="grid">
              <div class="col-12 md:col-6">
                <div class="field">
                  <label class="block text-sm font-semibold mb-2 text-color-secondary"
                    >Nome Completo</label
                  >
                  <div
                    v-if="!isEditing"
                    class="p-2 border-1 border-gray-300 border-round bg-surface-50"
                  >
                    {{ formData.nome || 'Não informado' }}
                  </div>
                  <p v-else class="m-0 text-color-secondary text-sm">
                    <i class="pi pi-info-circle mr-1"></i>
                    O nome não pode ser alterado pelo usuário
                  </p>
                </div>
              </div>

              <div class="col-12 md:col-6">
                <div class="field">
                  <label class="block text-sm font-semibold mb-2 text-color-secondary"
                    >E-mail</label
                  >
                  <div
                    v-if="!isEditing"
                    class="p-2 border-1 border-gray-300 border-round bg-surface-50 flex align-items-center"
                  >
                    {{ formData.email || 'Não informado' }}
                    <i
                      class="pi pi-verified text-green-500 ml-2"
                      v-tooltip="'E-mail verificado'"
                    ></i>
                  </div>
                  <p v-else class="m-0 text-color-secondary text-sm">
                    <i class="pi pi-info-circle mr-1"></i>
                    Para alterar o e-mail, entre em contato com o administrador
                  </p>
                </div>
              </div>

              <div class="col-12 md:col-6">
                <div class="field">
                  <label class="block text-sm font-semibold mb-2 text-color-secondary"
                    >Telefone</label
                  >
                  <InputMask
                    v-if="isEditing"
                    v-model="formData.telefone"
                    mask="(99) 99999-9999"
                    placeholder="(00) 00000-0000"
                    class="w-full"
                    size="small"
                  />
                  <div v-else class="p-2 border-1 border-gray-300 border-round bg-surface-50">
                    {{ formData.telefone || 'Não informado' }}
                  </div>
                </div>
              </div>

              <div class="col-12 md:col-6">
                <div class="field">
                  <label class="block text-sm font-semibold mb-2 text-color-secondary">CPF</label>
                  <InputMask
                    v-if="isEditing"
                    v-model="formData.cpf"
                    mask="999.999.999-99"
                    placeholder="000.000.000-00"
                    class="w-full"
                    size="small"
                  />
                  <div v-else class="p-2 border-1 border-gray-300 border-round bg-surface-50">
                    {{ formData.cpf || 'Não informado' }}
                  </div>
                </div>
              </div>
            </div>
          </template>
        </Card>

        <!-- Organizational Information -->
        <Card class="mb-4">
          <template #title>
            <div class="flex align-items-center gap-2">
              <i class="pi pi-building text-primary"></i>
              <span>Informações Organizacionais</span>
            </div>
          </template>
          <template #content>
            <div class="grid">
              <div class="col-12 md:col-6">
                <div class="field">
                  <label class="block text-sm font-semibold mb-2 text-color-secondary"
                    >Unidade Organizacional</label
                  >
                  <div class="p-2 border-1 border-gray-300 border-round bg-surface-50">
                    {{ unitOrganizationalFormatString(formData.unidade) }}
                  </div>
                </div>
              </div>

              <div class="col-12 md:col-6">
                <div class="field">
                  <label class="block text-sm font-semibold mb-2 text-color-secondary"
                    >Perfil de Acesso</label
                  >
                  <div class="p-1 border-1 border-gray-300 border-round bg-surface-50">
                    <Tag
                      :value="formData.role || 'Não definido'"
                      :severity="getRoleTagSeverity(formData.role || '')"
                    />
                  </div>
                </div>
              </div>
            </div>
          </template>
        </Card>

        <!-- Security Section -->
        <Card>
          <template #title>
            <div class="flex align-items-center gap-2">
              <i class="pi pi-shield text-primary"></i>
              <span>Segurança</span>
            </div>
          </template>
          <template #content>
            <div class="flex flex-column sm:flex-row gap-3">
              <Button
                label="Alterar Senha"
                icon="pi pi-key"
                severity="secondary"
                outlined
                @click="changePassword"
                class="flex-1 sm:flex-initial"
                style="min-width: 150px"
                size="small"
              />
              <Button
                label="Histórico de Login"
                icon="pi pi-history"
                severity="secondary"
                outlined
                class="flex-1 sm:flex-initial"
                style="min-width: 150px"
                size="small"
              />
            </div>

            <Message
              severity="warn"
              class="mt-4 p-2"
              size="small"
              icon="pi pi-exclamation-triangle"
            >
              <p>
                Altere sua senha regularmente e nunca compartilhe suas credenciais com terceiros.
              </p>
            </Message>
          </template>
        </Card>
      </div>
    </div>
  </div>
</template>

<style scoped>
.field {
  margin-bottom: 1rem;
}

.field:last-child {
  margin-bottom: 0;
}
</style>
