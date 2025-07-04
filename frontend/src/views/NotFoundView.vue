<script setup lang="ts">
import { useRouter } from 'vue-router'
import NotFoundSvg from '@/assets/NotFoundSvg.vue';

const router = useRouter();

const goBack = () => {
  router.back();
};
</script>

<template>
  <div class="page-container">
    <div class="content-wrapper">

      <div class="text-content">
        <h1 class="error-code">404</h1>
        <h2 class="error-title">Oops! Parece que você se perdeu.</h2>
        <p class="error-description">
          A página que você está procurando pode ter sido removida, renomeada ou nunca existiu.
        </p>

        <div class="actions-container">
          <router-link to="/" class="button primary">
            Ir para a Home
          </router-link>
          <button @click="goBack" class="button secondary">
            Voltar
          </button>
        </div>
      </div>

       <div class="illustration-container">
        <NotFoundSvg />
      </div>

    </div>
  </div>
</template>

<style scoped>

.page-container {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 80vh; /* Ocupa quase toda a altura da tela */
  padding: 2rem;
  font-family: 'Inter', sans-serif; /* Use uma fonte moderna, se disponível no seu projeto */
}

.content-wrapper {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  gap: 2rem;
}

/* Estilos da ilustração */
.illustration-container {
  max-width: 600px;
  width: 35rem;
}

.illustration {
  width: 100%;
  height: auto;
}

/* Estilos da área de texto */
.text-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.75rem;
}

.error-code {
  font-size: 6rem; /* Tamanho grande para o "404" */
  font-weight: 800; /* Super bold */
  line-height: 1;
  color: var(--p-primary-color, #4a90e2); /* Cor primária do seu tema */
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.1);
  margin: 0;
}

.error-title {
  font-size: 1.75rem;
  font-weight: 700;
  color: var(--p-text-color, #333);
  margin: 0;
}

.error-description {
  max-width: 450px;
  font-size: 1rem;
  color: var(--p-text-secondary-color, #6c757d);
  line-height: 1.6;
}

/* Estilos dos botões de ação */
.actions-container {
  display: flex;
  gap: 1rem;
  margin-top: 1.5rem;
}

.button {
  display: inline-block;
  padding: 0.75rem 1.5rem;
  border-radius: 8px;
  font-size: 1rem;
  font-weight: 600;
  text-decoration: none;
  cursor: pointer;
  border: 2px solid transparent;
  transition: all 0.3s ease;
  user-select: none; /* Impede a seleção de texto do botão */
}

.button.primary {
  background-color: var(--p-primary-color, #4a90e2);
  color: white;
}

.button.primary:hover {
  background-color: var(--p-primary-color-darker, #357abd);
  transform: translateY(-3px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.button.secondary {
  background-color: transparent;
  color: var(--p-primary-color, #4a90e2);
  border-color: var(--p-primary-color, #4a90e2);
}

.button.secondary:hover {
  background-color: var(--p-primary-color-lighter, #e9f2fc);
  transform: translateY(-3px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

/* Design Responsivo para telas maiores (desktop) */
@media (min-width: 768px) {
  .content-wrapper {
    flex-direction: row; /* Lado a lado no desktop */
    text-align: left;
    gap: 4rem;
  }
  
  .text-content {
    align-items: flex-start;
  }

  .actions-container {
    justify-content: flex-start;
  }

  .error-code {
    font-size: 8rem;
  }

  .error-title {
    font-size: 2.25rem;
  }
}
</style>