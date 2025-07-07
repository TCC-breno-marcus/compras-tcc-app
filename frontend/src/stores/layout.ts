import { ref } from 'vue'
import { defineStore } from 'pinia'

export const useLayoutStore = defineStore(
  'layout',
  () => {
    const isSidebarCollapsed = ref(false)

    function toggleSidebar() {
      isSidebarCollapsed.value = !isSidebarCollapsed.value
    }

    return { isSidebarCollapsed, toggleSidebar }
  },
  {
    persist: true,
  },
)
