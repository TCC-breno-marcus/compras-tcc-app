<script setup lang="ts">
import { ref, watch } from 'vue'
import { useAuthStore } from '@/features/autentication/stores/authStore'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import FloatLabel from 'primevue/floatlabel'
import { useToast } from 'primevue/usetoast'
import { useRouter } from 'vue-router'
import Password from 'primevue/password'
import { Dialog, Message } from 'primevue'
import { reactive } from 'vue'
import InputMask from 'primevue/inputmask'
import Select from 'primevue/select'
import { cpfValidator, emailValidator } from '@/utils/validators'
import { storeToRefs } from 'pinia'

const props = defineProps<{
  visible: boolean
}>()

const emit = defineEmits(['update:visible', 'update:user-list'])

const authStore = useAuthStore()
const { departamentos } = storeToRefs(authStore)
const toast = useToast()
const email = ref('')
const password = ref('')
const nome = ref('')
const telefone = ref('')
const cpf = ref('')
const departamento = ref(null)
const isLoading = ref(false)

const errors = reactive({
  nome: '',
  email: '',
  telefone: '',
  cpf: '',
  password: '',
  departamento: '',
})

const resetErrors = () => {
  errors.email = ''
  errors.password = ''
  errors.nome = ''
  errors.telefone = ''
  errors.cpf = ''
  errors.departamento = ''
}

const validateForm = (): boolean => {
  resetErrors()
  let isValid = true

  if (!email.value) {
    errors.email = 'O email é obrigatório.'
    isValid = false
  } else if (!emailValidator(email.value)) {
    errors.email = 'O formato do email é inválido.'
    isValid = false
  }

  if (!password.value) {
    errors.password = 'A senha é obrigatória.'
    isValid = false
  }

  if (!nome.value) {
    errors.nome = 'O nome é obrigatório.'
    isValid = false
  }

  if (!telefone.value) {
    errors.telefone = 'O telefone é obrigatório.'
    isValid = false
  }

  if (!departamento.value) {
    errors.departamento = 'O departamento é obrigatório.'
    isValid = false
  }

  if (!cpf.value) {
    errors.cpf = 'O CPF é obrigatório.'
    isValid = false
  } else if (!cpfValidator(cpf.value)) {
    errors.cpf = 'O CPF informado é inválido.'
    isValid = false
  }

  return isValid
}

const handleRegister = async () => {
  if (!validateForm()) {
    return
  }

  isLoading.value = true
  try {
    await authStore.register({
      email: email.value,
      password: password.value,
      nome: nome.value,
      telefone: telefone.value.replace(/\D/g, ''),
      cpf: cpf.value.replace(/\D/g, ''),
      departamento: departamento.value!,
    })

    toast.add({
      severity: 'success',
      summary: 'Sucesso',
      detail: 'Usuário criado com sucesso.',
      life: 3000,
    })
    resetAllStates()
    emit('update:visible', false)
    emit('update:user-list')
  } catch (error: unknown) {
    toast.add({
      severity: 'error',
      summary: 'Erro',
      detail: error,
      life: 3000,
    })
  } finally {
    isLoading.value = false
  }
}

const resetAllStates = () => {
  resetErrors()
  email.value = ''
  password.value = ''
  nome.value = ''
  telefone.value = ''
  cpf.value = ''
  departamento.value = null
  isLoading.value = false
}

const closeModal = () => {
  resetAllStates()
  emit('update:visible', false)
}

watch(
  () => props.visible,
  (isNowVisible: boolean) => {
    if (isNowVisible) {
      resetAllStates()
      if (departamentos.value.length == 0) {
        authStore.fetchDeptos()
      }
    }
  },
)

watch(
  [email, password, nome, telefone, cpf, departamento],
  ([newEmail, newPassword, newNome, newTelefone, newCpf, newDepartamento]) => {
    if (newEmail) {
      errors.email = ''
    }
    if (newPassword) {
      errors.password = ''
    }
    if (newNome) {
      errors.nome = ''
    }
    if (newTelefone) {
      errors.telefone = ''
    }
    if (newCpf) {
      errors.cpf = ''
    }
    if (newDepartamento) {
      errors.departamento = ''
    }
  },
)
</script>

<template>
  <Dialog
    :visible="visible"
    @update:visible="closeModal"
    modal
    :header="`Criar Novo Usuário`"
    :style="{ width: '90vw', maxWidth: '500px' }"
  >
    <form class="flex flex-column gap-3">
      <div class="flex align-items-center mb-3 gap-3">
        <i class="pi pi-user-plus text-2xl text-primary"></i>
        <p class="text-color-secondary">
          Preencha os dados abaixo. O perfil padrão para novos usuários será "Solicitante".
        </p>
      </div>
      <div class="flex flex-column gap-1">
        <FloatLabel variant="on">
          <InputText
            name="email"
            v-model="email"
            type="email"
            class="w-full"
            size="small"
            :invalid="!!errors.email"
            autocomplete="off"
          />
          <label for="email">Email</label>
        </FloatLabel>
        <Message v-if="errors.email" class="ml-1" severity="error" size="small" variant="simple"
          >{{ errors.email }}
        </Message>
      </div>
      <div class="flex flex-column gap-1">
        <FloatLabel variant="on">
          <Password
            id="password"
            v-model="password"
            :feedback="false"
            toggleMask
            fluid
            :invalid="!!errors.password"
            autocomplete="new-password"
            size="small"
          />
          <label for="password">Senha</label>
        </FloatLabel>
        <Message v-if="errors.password" class="ml-1" severity="error" size="small" variant="simple"
          >{{ errors.password }}
        </Message>
      </div>
      <div class="flex flex-column gap-1">
        <FloatLabel variant="on">
          <InputText
            name="nome"
            v-model="nome"
            type="nome"
            class="w-full"
            size="small"
            :invalid="!!errors.nome"
          />
          <label for="nome">Nome</label>
        </FloatLabel>
        <Message v-if="errors.nome" class="ml-1" severity="error" size="small" variant="simple"
          >{{ errors.nome }}
        </Message>
      </div>
      <div class="flex flex-column gap-1">
        <FloatLabel variant="on">
          <InputMask
            id="cpf"
            name="cpf"
            v-model="cpf"
            mask="999.999.999-99"
            class="w-full"
            size="small"
            :invalid="!!errors.cpf"
          />
          <label for="cpf">CPF</label>
        </FloatLabel>
        <Message v-if="errors.cpf" class="ml-1" severity="error" size="small" variant="simple"
          >{{ errors.cpf }}
        </Message>
      </div>
      <div class="flex flex-column gap-1">
        <FloatLabel variant="on">
          <InputMask
            id="telefone"
            name="telefone"
            v-model="telefone"
            mask="(99) 99999-9999"
            class="w-full"
            size="small"
            :invalid="!!errors.telefone"
          />
          <label for="telefone">Telefone</label>
        </FloatLabel>
        <Message v-if="errors.telefone" class="ml-1" severity="error" size="small" variant="simple"
          >{{ errors.telefone }}
        </Message>
      </div>
      <div class="flex flex-column gap-1">
        <FloatLabel variant="on">
          <Select
            v-model="departamento"
            :options="departamentos"
            inputId="departamento"
            size="small"
            class="w-full"
            id="departamento"
            :invalid="!!errors.departamento"
            :showClear="true"
            filter
          />
          <label for="departamento">Departamento</label>
        </FloatLabel>
        <Message
          v-if="errors.departamento"
          class="ml-1"
          severity="error"
          size="small"
          variant="simple"
          >{{ errors.departamento }}
        </Message>
      </div>
    </form>

    <template #footer>
      <Button
        label="Cancelar"
        severity="danger"
        icon="pi pi-times"
        @click="closeModal"
        text
        size="small"
        :disabled="isLoading"
      />
      <Button
        icon="pi pi-user-plus"
        label="Criar"
        :loading="isLoading"
        size="small"
        @click="handleRegister"
      />
    </template>
  </Dialog>
</template>

<style scoped></style>
