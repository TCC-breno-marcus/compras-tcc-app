import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import AppLayout from '@/layouts/AppLayout.vue'
import NotFoundView from '@/views/NotFoundView.vue'
import NewSolicitation from '@/features/solicitations/views/NewSolicitation.vue'
import ManagerPanel from '@/features/management/views/ManagerPanelView.vue'
import SolicitationDetailsView from '@/features/solicitations/views/SolicitationDetailsView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      component: AppLayout, // O Layout é o componente da rota pai
      children: [
        // As páginas são "filhas" do layout
        {
          path: '/solicitacoes/criar',
          name: 'NewSolicitation',
          component: NewSolicitation,
        },
        {
          path: '/solicitacoes/:id',
          name: 'SolicitationDetails', // Dê um nome à rota
          component: SolicitationDetailsView,
        },
        {
          path: '/gestor',
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
          component: HomeView,
        },
        {
          path: '/:pathMatch(.*)*', // Regex que captura qualquer coisa
          name: 'NotFound',
          component: NotFoundView,
        },
        {
          path: '/error',
          name: 'ServerError',
          component: () => import('../views/ServerErrorView.vue'),
        },
        // ... outras rotas que usam o mesmo layout
      ],
    },
    // {
    //   path: '/login', // Uma rota que NÃO usa o AppLayout
    //   name: 'login',
    //   component: () => import('../views/LoginView.vue')
    // }
  ],
})

export default router
