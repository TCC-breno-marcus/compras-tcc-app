<script setup lang="ts">
import Button from 'primevue/button'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/features/autentication/stores/authStore'

const router = useRouter()
const authStore = useAuthStore()

const goToDashboard = () => {
  // Envia para uma rota segura, como o dashboard
  router.push('/gestor/dashboard')
}

const handleLogout = () => {
  authStore.logout()
}
</script>

<template>
  <div class="error-page flex align-items-center justify-content-center h-screen text-center">
    <div class="surface-card p-5 border-round w-full max-w-lg">
      <div class="error-icon mb-4">
        <i class="pi pi-lock text-6xl text-orange-500"></i>
      </div>
      <h1 class="text-4xl font-bold text-900 mt-0 mb-2">Acesso Negado</h1>
      <p class="text-color-secondary text-lg mt-0 mb-4">
        Você não tem permissão para acessar esta página.
      </p>
      <p class="text-color-secondary mb-5">
        Se você acredita que isso é um erro, entre em contato com o administrador do sistema.
      </p>
      <div class="flex justify-content-center gap-2">
        <Button
          size="small"
          label="Voltar ao Painel"
          icon="pi pi-arrow-left"
          @click="goToDashboard"
        ></Button>
        <Button
          size="small"
          label="Fazer Logout"
          icon="pi pi-sign-out"
          severity="secondary"
          outlined
          @click="handleLogout"
        ></Button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.error-page {
  background: var(--surface-100);
}
.error-icon i {
  text-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
}
</style>
