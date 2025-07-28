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
      command: () => {
        router.push('/gestor/dashboard')
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
