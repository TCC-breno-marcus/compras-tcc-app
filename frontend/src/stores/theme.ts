import { ref, computed } from 'vue'
import { defineStore } from 'pinia'

type Theme = 'light' | 'dark'

// TODO: validar todas as paginas pra ajustar o tema escuro

/**
 * Store global para controle de tema da interface.
 */
export const useThemeStore = defineStore(
  'theme',
  () => {
    const currentTheme = ref<Theme>('light')

    const isDarkMode = computed(() => currentTheme.value === 'dark')

    /**
     * Alterna entre tema claro e escuro.
     */
    const toggleTheme = () => {
      currentTheme.value = currentTheme.value === 'light' ? 'dark' : 'light'
    }

    /**
     * Define explicitamente o tema ativo.
     * @param theme Tema alvo.
     */
    const setTheme = (theme: Theme) => {
      currentTheme.value = theme
    }

    return { currentTheme, isDarkMode, toggleTheme, setTheme }
  },
  {
    persist: true,
  },
)
