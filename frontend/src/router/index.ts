import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import AppLayout from '@/layouts/AppLayout.vue'
import NotFoundView from '@/views/NotFoundView.vue'
import NewGeneralSolicitationView from '@/features/solicitations/views/NewGeneralSolicitationView.vue'
import NewPatrimonialSolicitationView from '@/features/solicitations/views/NewPatrimonialSolicitationView.vue'
import ManagerPanel from '@/features/management/views/ManagerPanelView.vue'
import SolicitationDetailsView from '@/features/solicitations/views/SolicitationDetailsView.vue'
import { useAuthStore } from '@/stores/authStore'
import MySolicitationsView from '@/features/solicitations/views/MySolicitationsView.vue'

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
              meta: { requiresAuth: true, roles: ['Solicitante', 'Admin'] },
            },
            {
              path: 'criar/patrimonial',
              component: NewPatrimonialSolicitationView,
              meta: { requiresAuth: true, roles: ['Solicitante', 'Admin'] },
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
          component: () => import('../views/UnauthorizedView.vue'),
        },
      ],
    },
    {
      path: '/login',
      name: 'Login',
      component: () => import('../views/LoginView.vue'),
    },
    {
      path: '/register',
      name: 'Register',
      component: () => import('../views/RegisterView.vue'),
    },
    {
      path: '/:pathMatch(.*)*',
      name: 'NotFound',
      component: NotFoundView,
    },
  ],
})

// 2. Navigation Guard Global
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return next({ name: 'Login', query: { redirect: to.fullPath } })
  }

  if (to.meta.roles && !to.meta.roles.includes(authStore.user?.role ?? '')) {
    return next({ name: 'Unauthorized' })
  }

  return next()
})

export default router
