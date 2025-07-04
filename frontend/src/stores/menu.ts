import { ref } from 'vue'
import { defineStore } from 'pinia'
import { useRouter } from 'vue-router'

export const useMenuStore = defineStore('menu', () => {
  const router = useRouter()

  const itemsMenu = ref([
    {
      label: 'Solicitações',
      icon: 'assignment',
      materialIcon: true,
      items: [
        {
          label: 'Solicitações',
          class: 'submenu-title',
          disabled: true,
        },
        {
          label: 'Nova Solicitação',
          icon: 'add',
          materialIcon: true,
          route: '/solicitacoes/criar',
        },
        {
          label: 'Minhas Solicitações',
          icon: 'assignment_ind',
          materialIcon: true,
          route: '/solicitacoes/minhas',
        },
      ],
    },
    {
      label: 'Painel do Gestor',
      icon: 'bar_chart',
      materialIcon: true,
      command: () => {
        router.push('/painel-gestor')
      },
    },
    {
      label: 'Fale Conosco',
      icon: 'mail',
      materialIcon: true,
      command: () => {
        router.push('/fale-conosco')
      },
    },
  ])

  const itemsMenuOverlay = ref([
    {
      label: 'Solicitações',
      items: [
        {
          label: 'Nova Solicitação',
          icon: 'add_shopping_cart',
          route: '/solicitacoes/criar',
        },
        {
          label: 'Minhas Solicitações',
          icon: 'list_alt',
          route: '/solicitacoes/minhas',
        },
      ],
    },
    {
      label: 'Gestor',
      items: [
        {
          label: 'Painel do Gestor',
          icon: 'bar_chart',
          command: () => {
            router.push('/painel-gestor')
          },
        }
      ],
    },
    {
      label: 'Contato',
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

  return { itemsMenu, itemsMenuOverlay }
})
