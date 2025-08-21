import { ref, computed } from 'vue'
import { defineStore } from 'pinia'

type Theme = 'light' | 'dark'

export const useThemeStore = defineStore(
  'theme',
  () => {
    const currentTheme = ref<Theme>('light')

    const isDarkMode = computed(() => currentTheme.value === 'dark')

    const toggleTheme = () => {
      currentTheme.value = currentTheme.value === 'light' ? 'dark' : 'light'
    }

    const setTheme = (theme: Theme) => {
      currentTheme.value = theme
    }

    return { currentTheme, isDarkMode, toggleTheme, setTheme }
  },
  {
    persist: true,
  },
)
