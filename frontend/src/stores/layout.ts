import { ref, computed, onMounted, onUnmounted } from 'vue'
import { defineStore } from 'pinia'

/**
 * Store global para estado de layout responsivo e colapso da sidebar.
 */
export const useLayoutStore = defineStore(
  'layout',
  () => {
    const isSidebarCollapsed = ref(false)

    /**
     * Alterna visibilidade da barra lateral.
     */
    const toggleSidebar = () => {
      isSidebarCollapsed.value = !isSidebarCollapsed.value
    }

    const screenWidth = ref(window.innerWidth)

    const currentBreakpoint = computed(() => {
      const width = screenWidth.value
      if (width < 576) return 'xs'
      if (width < 768) return 'sm'
      if (width < 992) return 'md'
      if (width < 1200) return 'lg'
      return 'xl'
    })

    /**
     * Atualiza breakpoint corrente com base na largura da janela.
     */
    const handleResize = () => {
      screenWidth.value = window.innerWidth
    }

    onMounted(() => {
      window.addEventListener('resize', handleResize)
    })

    onUnmounted(() => {
      window.removeEventListener('resize', handleResize)
    })

    return { isSidebarCollapsed, toggleSidebar, currentBreakpoint }
  },
  {
    persist: true,
  },
)
