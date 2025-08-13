<script setup lang="ts">
import { RouterView } from 'vue-router'
import { watchEffect } from 'vue'
import { useThemeStore } from './stores/theme'
import Toast from 'primevue/toast'
import ConfirmDialog from 'primevue/confirmdialog'

const themeStore = useThemeStore()

watchEffect(() => {
  const rootElement = document.documentElement
  if (themeStore.currentTheme === 'dark') {
    rootElement.classList.add('p-dark')
  } else {
    rootElement.classList.remove('p-dark')
  }
})

</script>

<template>
  <Toast />
  <Toast group="loading" />
  <ConfirmDialog />
  <RouterView />
</template>
