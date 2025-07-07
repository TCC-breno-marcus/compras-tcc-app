import { ref, computed } from 'vue'
import { defineStore } from 'pinia'

// Definimos os tipos para segurança
type Theme = 'light' | 'dark'

export const useThemeStore = defineStore(
  'theme',
  () => {
    const currentTheme = ref<Theme>('light')

    const isDarkMode = computed(() => currentTheme.value === 'dark')

    function toggleTheme() {
      currentTheme.value = currentTheme.value === 'light' ? 'dark' : 'light'
    }

    function setTheme(theme: Theme) {
      currentTheme.value = theme
    }

    return { currentTheme, isDarkMode, toggleTheme, setTheme }
  },
  {
    persist: true,
  },
)
