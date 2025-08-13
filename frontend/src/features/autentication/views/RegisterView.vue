<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useAuthStore } from '@/features/autentication/stores/authStore'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import FloatLabel from 'primevue/floatlabel'
import { useToast } from 'primevue/usetoast'
import { useRouter } from 'vue-router'
import Logo from '@/components/ui/Logo.vue'
import Password from 'primevue/password'
import { Message } from 'primevue'
import { reactive } from 'vue'
import InputMask from 'primevue/inputmask'
import Select from 'primevue/select'
import { cpfValidator, emailValidator } from '@/utils/validators'
import { storeToRefs } from 'pinia'

const router = useRouter()
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
    errors.telefone = 'O departamento é obrigatório.'
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
    const hasRegistered = await authStore.register({
      email: email.value,
      password: password.value,
      nome: nome.value,
      telefone: telefone.value.replace(/\D/g, ''),
      cpf: cpf.value.replace(/\D/g, ''),
      departamento: departamento.value!,
    })
    if (hasRegistered) {
      toast.add({
        severity: 'success',
        summary: 'Sucesso',
        detail: 'Usuário criado com sucesso.',
        life: 3000,
      })
      router.push('/')
    } else {
      toast.add({
        severity: 'error',
        summary: 'Erro',
        detail: 'Não foi possível criar o usuário.',
        life: 3000,
      })
    }
  } catch (error: any) {
    toast.add({
      severity: 'error',
      summary: 'Erro',
      detail: error.message,
      life: 3000,
    })
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  authStore.fetchDeptos()
})
</script>

<template>
  <div class="flex align-items-center justify-content-center h-screen bg-surface-50">
    <div class="card-login">
      <div class="header-login">
        <Logo />
      </div>

      <div class="form-container">
        <form @submit.prevent="handleRegister" class="flex flex-column gap-3">
          <h2 class="font-bold text-center text-xl">Criar uma conta</h2>

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
              />
              <label for="password">Senha</label>
            </FloatLabel>
            <Message
              v-if="errors.password"
              class="ml-1"
              severity="error"
              size="small"
              variant="simple"
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
            <Message
              v-if="errors.telefone"
              class="ml-1"
              severity="error"
              size="small"
              variant="simple"
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
              />
              <label for="departamento"><Div></Div>Departamento</label>
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
          <Button
            type="submit"
            icon="pi pi-user-plus"
            label="Registrar"
            :loading="isLoading"
            class="w-full mt-2"
          />
          <router-link to="/login" class="text-center text-sm text-primary-500 hover:underline">
            Já tem uma conta? Faça login
          </router-link>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
.card-login {
  background: var(--surface-card);
  border-radius: 12px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
  overflow: hidden;
  max-width: 450px;
  width: 90%;
}

.header-login {
  padding: 1.5rem;
}

.form-container {
  padding: 2.5rem;
}

h2 {
  margin-top: 0;
  margin-bottom: 0.5rem;
}
</style>
