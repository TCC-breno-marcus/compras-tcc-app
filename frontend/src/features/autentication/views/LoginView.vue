<script setup lang="ts">
import { ref } from 'vue'
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

const router = useRouter()
const authStore = useAuthStore()
const toast = useToast()

const email = ref('')
const password = ref('')
const isLoading = ref(false)

const errors = reactive({
  email: '',
  password: '',
})

const validateForm = (): boolean => {
  errors.email = ''
  errors.password = ''
  let isValid = true

  if (!email.value) {
    errors.email = 'O email é obrigatório.'
    isValid = false
  }

  if (!password.value) {
    errors.password = 'A senha é obrigatória.'
    isValid = false
  }

  return isValid
}

const handleLogin = async () => {
  if (!validateForm()) {
    return
  }

  isLoading.value = true
  try {
    await authStore.login({
      email: email.value,
      password: password.value,
    })
    await authStore.fetchDataUser()
    router.push('/')
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
</script>

<template>
  <div class="flex align-items-center justify-content-center h-screen bg-surface-50">
    <div class="card-login">
      <div class="header-login">
        <Logo />
      </div>

      <div class="form-container">
        <form @submit.prevent="handleLogin" class="flex flex-column gap-3">
          <h2 class="font-bold text-center text-xl">Acesso ao Sistema</h2>

          <div class="flex flex-column gap-1">
            <FloatLabel variant="on">
              <InputText
                name="email"
                v-model="email"
                type="email"
                class="w-full"
                size="small"
                :invalid="!!errors.email"
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
          <Button
            type="submit"
            icon="pi pi-sign-in"
            label="Entrar"
            :loading="isLoading"
            class="w-full mt-2"
          />
          <p class="text-center text-color-secondary">
            Ainda não possui acesso? Solicite junto à gestão do seu centro ou departamento.
          </p>
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
