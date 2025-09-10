<script setup lang="ts">
import Button from 'primevue/button';
import { useRouter } from 'vue-router';

const router = useRouter();

const goToHome = () => {
  router.push('/');
};
</script>

<template>
  <div class="error-page flex align-items-center justify-content-center h-screen text-center">
    <div class="surface-card p-5 border-round w-full max-w-lg">
      <div class="error-icon mb-4">
        <i class="pi pi-server text-6xl text-red-500"></i>
      </div>
      <h1 class="text-4xl font-bold text-900 mt-0 mb-2">Erro Interno do Servidor</h1>
      <p class="text-color-secondary mb-5">Por favor, tente novamente mais tarde ou volte para a página inicial.</p>
      <Button label="Início" icon="pi pi-home" @click="goToHome"></Button>
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