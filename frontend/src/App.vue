<script setup lang="ts">
import { RouterView } from 'vue-router'
import { watchEffect } from 'vue';
import { useThemeStore } from './stores/theme';

const themeStore = useThemeStore();

// 2. Cria um 'observador' que reage a qualquer mudança em themeStore.currentTheme
watchEffect(() => {
  const rootElement = document.documentElement; 
  if (themeStore.currentTheme === 'dark') {
    rootElement.classList.add('p-dark');
  } else {
    rootElement.classList.remove('p-dark');
  }
});
</script>

<template>
  <RouterView />
</template>