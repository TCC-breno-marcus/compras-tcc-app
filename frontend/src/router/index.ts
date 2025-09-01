import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import AppLayout from '@/layouts/AppLayout.vue'
import NotFoundView from '@/views/NotFoundView.vue'
import NewGeneralSolicitationView from '@/features/solicitations/views/NewGeneralSolicitationView.vue'
import NewPatrimonialSolicitationView from '@/features/solicitations/views/NewPatrimonialSolicitationView.vue'
import ManagerPanel from '@/features/management/views/ManagerPanelView.vue'
import SolicitationDetailsView from '@/features/solicitations/views/SolicitationDetailsView.vue'
import { useAuthStore } from '@/features/autentication/stores/authStore'
import MySolicitationsView from '@/features/solicitations/views/MySolicitationsView.vue'
import { useSolicitationCartStore } from '@/features/solicitations/stores/solicitationCartStore'
import { useConfirm } from 'primevue'
import { CHANGE_SOLICITATION_CONFIRMATION } from '@/utils/confirmationFactoryUtils'
import SettingsView from '@/features/settings/views/SettingsView.vue'
import NotificationsSettings from '@/features/settings/components/NotificationsSettings.vue'
import GeneralSettings from '@/features/settings/components/GeneralSettings.vue'
import SolicitationsSettings from '@/features/settings/components/SolicitationsSettings.vue'
import UsersAndPermissionsSettings from '@/features/settings/components/UsersAndPermissionsSettings.vue'

declare module 'vue-router' {
  interface RouteMeta {
    requiresAuth: boolean
    roles?: string[]
  }
}

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      component: AppLayout,
      children: [
        {
          path: '/solicitacoes',
          children: [
            {
              path: 'criar/geral',
              component: NewGeneralSolicitationView,
              meta: {
                requiresAuth: true,
                roles: ['Solicitante', 'Admin'],
                isCreateSolicitationPage: true,
                solicitationType: 'geral',
              },
            },
            {
              path: 'criar/patrimonial',
              component: NewPatrimonialSolicitationView,
              meta: {
                requiresAuth: true,
                roles: ['Solicitante', 'Admin'],
                isCreateSolicitationPage: true,
                solicitationType: 'patrimonial',
              },
            },
            {
              path: ':id',
              name: 'SolicitationDetails',
              component: SolicitationDetailsView,
              meta: { requiresAuth: true, roles: ['Solicitante', 'Gestor', 'Admin'] },
            },
            {
              path: '',
              name: 'MySolicitations',
              component: MySolicitationsView,
              meta: { requiresAuth: true, roles: ['Solicitante', 'Gestor', 'Admin'] },
            },
          ],
        },
        {
          path: '/gestor',
          meta: { requiresAuth: true, roles: ['Gestor', 'Admin'] },
          component: ManagerPanel,
          children: [
            {
              path: 'dashboard',
              component: () => import('@/features/management/components/Dashboard.vue'),
            },
            {
              path: 'departamento',
              component: () => import('@/features/management/components/ItemsPerDepartment.vue'),
            },
            {
              path: 'solicitacoes',
              component: () => import('@/features/management/components/Solicitations.vue'),
            },
            {
              path: 'catalogo',
              component: () => import('@/features/management/components/ManageItems.vue'),
            },
            {
              path: 'relatorios',
              component: () => import('@/features/management/components/Reports.vue'),
            },
          ],
        },
        {
          path: '/configuracoes',
          meta: { requiresAuth: true, roles: ['Gestor', 'Admin'] },
          component: SettingsView,
          children: [
            {
              path: 'geral',
              component: GeneralSettings,
            },
            {
              path: 'solicitacoes',
              component: SolicitationsSettings,
            },
            {
              path: 'usuarios',
              component: UsersAndPermissionsSettings,
            },
            {
              path: 'notificacoes',
              component: NotificationsSettings,
            },
          ],
        },
        {
          path: '',
          name: 'Home',
          meta: { requiresAuth: true, roles: ['Solicitante', 'Gestor', 'Admin'] },
          component: HomeView,
        },
        {
          path: '/error',
          name: 'ServerError',
          component: () => import('../views/ServerErrorView.vue'),
        },
        {
          path: '/unauthorized',
          name: 'Unauthorized',
          component: () => import('../features/autentication/views/UnauthorizedView.vue'),
        },
        {
          path: '/not-found',
          name: 'AppNotFound',
          component: NotFoundView,
          meta: { requiresAuth: true },
        },
      ],
    },
    {
      path: '/login',
      name: 'Login',
      component: () => import('../features/autentication/views/LoginView.vue'),
    },
    {
      path: '/register',
      name: 'Register',
      component: () => import('../features/autentication/views/RegisterView.vue'),
    },
    {
      path: '/not-found',
      name: 'PublicNotFound',
      component: NotFoundView,
    },
  ],
})

// 2. Navigation Guard Global
router.beforeEach((to, from, next) => {
  const solicitationCartStore = useSolicitationCartStore()
  const confirm = useConfirm()

  const hasExistingSolicitation = solicitationCartStore.solicitationItems.length > 0
  const isEnteringCreatePage = to.meta.isCreateSolicitationPage === true
  const isDifferentType =
    solicitationCartStore.solicitationType !== null &&
    solicitationCartStore.solicitationType !== to.meta.solicitationType

  if (isEnteringCreatePage && hasExistingSolicitation && isDifferentType) {
    confirm.require({
      ...CHANGE_SOLICITATION_CONFIRMATION,
      message: `Você tem uma solicitação ${solicitationCartStore.solicitationType} em andamento. Deseja descartá-la para iniciar uma nova solicitação ${to.meta.solicitationType}?`,
      accept: () => {
        solicitationCartStore.$reset()
        router.push(to.fullPath)
      },
    })

    return next(false)
  }

  const authStore = useAuthStore()

  if (to.matched.length === 0) {
    if (authStore.isAuthenticated) {
      return next({ name: 'AppNotFound' })
    } else {
      return next({ name: 'PublicNotFound' })
    }
  }

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return next({ name: 'Login', query: { redirect: to.fullPath } })
  }

  if (to.meta.roles && !to.meta.roles.includes(authStore.user?.role ?? '')) {
    return next({ name: 'Unauthorized' })
  }

  return next()
})

export default router
