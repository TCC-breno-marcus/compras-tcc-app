<script setup lang="ts">
import { RouterView } from 'vue-router'
import { onMounted, watchEffect } from 'vue'
import { useThemeStore } from './stores/theme'
import Toast from 'primevue/toast'
import ConfirmDialog from 'primevue/confirmdialog'
import { useAuthStore } from './features/autentication/stores/authStore'
import { useSettingStore } from './features/settings/stores/settingStore'

const themeStore = useThemeStore()
const authStore = useAuthStore()
const settingsStore = useSettingStore()

onMounted(() => {
  if (authStore.isAuthenticated) {
    settingsStore.fetchSettings()
  }
})

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
