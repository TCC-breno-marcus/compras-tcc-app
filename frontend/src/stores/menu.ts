import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/features/autentication/stores/authStore'
import type { MenuItem } from 'primevue/menuitem'

export interface AppMenuItem extends MenuItem {
  roles?: string[]
  materialIcon?: boolean
  items?: AppMenuItem[]
}

export const useMenuStore = defineStore('menu', () => {
  const router = useRouter()
  const authStore = useAuthStore()

  const baseItemsMenu = ref<AppMenuItem[]>([
    {
      label: 'Solicitações',
      icon: 'assignment',
      materialIcon: true,
      roles: ['Solicitante', 'Admin'],
      items: [
        {
          label: 'Criar Solicitação',
          disabled: true,
          class: 'submenu-title',
        },
        {
          label: 'Geral',
          icon: 'assignment',
          materialIcon: true,
          command: () => {
            router.push('/solicitacoes/criar/geral')
          },
        },
        {
          label: 'Bens Patrimoniais',
          icon: 'chair',
          materialIcon: true,
          command: () => {
            router.push('/solicitacoes/criar/patrimonial')
          },
        },
        {
          label: 'Solicitações',
          disabled: true,
          class: 'submenu-title',
        },
        {
          label: 'Minhas Solicitações',
          icon: 'assignment_ind',
          materialIcon: true,
          command: () => {
            router.push('/solicitacoes')
          },
        },
      ],
    },
    {
      label: 'Painel do Gestor',
      icon: 'bar_chart',
      materialIcon: true,
      roles: ['Admin', 'Gestor'],
      command: () => {
        router.push('/gestor/dashboard')
      },
    },
    {
      label: 'Fale Conosco',
      icon: 'mail',
      materialIcon: true,
      roles: ['Admin', 'Gestor', 'Solicitante'],
      command: () => {
        router.push('/fale-conosco')
      },
    },
  ])

  const baseItemsMenuOverlay = ref<AppMenuItem[]>([
    {
      label: 'Solicitações',
      roles: ['Admin', 'Solicitante'],
      items: [
        {
          label: 'Solicitação Geral',
          icon: 'assignment',
          command: () => {
            router.push('/solicitacoes/criar/geral')
          },
        },
        {
          label: 'Bens Patrimoniais',
          icon: 'chair',
          command: () => {
            router.push('/solicitacoes/criar/patrimonial')
          },
        },
        {
          label: 'Minhas Solicitações',
          icon: 'assignment_ind',
          command: () => {
            router.push('/solicitacoes')
          },
        },
      ],
    },
    {
      label: 'Gestor',
      roles: ['Admin', 'Gestor'],
      items: [
        {
          label: 'Painel do Gestor',
          icon: 'bar_chart',
          command: () => {
            router.push('/gestor/dashboard')
          },
        },
      ],
    },
    {
      label: 'Contato',
      roles: ['Admin', 'Gestor'],
      items: [
        {
          label: 'Fale Conosco',
          icon: 'mail',
          materialIcon: true,
          command: () => {
            router.push('/fale-conosco')
          },
        },
      ],
    },
  ])

  /**
   * Filtra um array de itens de menu com base na role do usuário atual.
   * A função é recursiva para lidar com sub-menus.
   * @param items Array de itens de menu a serem filtrados.
   */
  const filterMenuByRole = (items: AppMenuItem[]): AppMenuItem[] => {
    const userRole = authStore.user?.role

    const filteredItems = []

    for (const item of items) {
      const isVisible = !item.roles || item.roles.includes(userRole as string)

      if (isVisible) {
        let filteredSubItems = item.items ? filterMenuByRole(item.items) : undefined
        if (
          item.class === 'submenu-title' &&
          (!filteredSubItems || filteredSubItems.length === 0)
        ) {
          continue
        }
        const newItem = { ...item }

        if (filteredSubItems) {
          newItem.items = filteredSubItems
        }
        filteredItems.push(newItem)
      }
    }

    return filteredItems
  }

  const itemsMenu = computed(() => filterMenuByRole(baseItemsMenu.value))
  const itemsMenuOverlay = computed(() => filterMenuByRole(baseItemsMenuOverlay.value))
  return { itemsMenu, itemsMenuOverlay }
})
