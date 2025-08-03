<script setup lang="ts">
import { useRouter } from 'vue-router'
import NotFoundSvg from '@/assets/NotFoundSvg.vue';

const router = useRouter();

const goBack = () => {
  router.back();
};
</script>

<template>
  <div class="flex align-items-center justify-content-center p-8" style="min-height: 100vh;">
    <div class="flex flex-column lg:flex-row align-items-center lg:justify-content-center text-center lg:text-left gap-2">

      <div class="flex flex-column align-items-center lg:align-items-start gap-2">
        <h1 class="error-code text-8xl font-bold line-height-1 m-0">404</h1>
        <h2 class="error-title font-3xl sm:font-5xl font-bold m-0">Oops! Parece que você se perdeu.</h2>
        <p class="error-description text-base">
          A página que você está procurando pode ter sido removida, renomeada ou nunca existiu.
        </p>

        <div class="flex gap-2 mt-1 sm:justify-content-start">
          <router-link to="/" class="button primary">
            Ir para a Home
          </router-link>
          <button @click="goBack" class="button secondary">
            Voltar
          </button>
        </div>
      </div>

       <div class="w-20rem sm:w-30rem">
        <NotFoundSvg />
      </div>

    </div>
  </div>
</template>

<style scoped>


.illustration {
  width: 100%;
  height: auto;
}

.error-code {
  color: var(--p-primary-color, #4a90e2); 
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.1);
}

.error-title {
  color: var(--p-text-color, #333);
}

.error-description {
  max-width: 450px;
  color: var(--p-text-secondary-color, #6c757d);
  line-height: 1.6;
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
  user-select: none; 
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

</style>