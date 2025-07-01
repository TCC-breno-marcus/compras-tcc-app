import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import AppLayout from '@/layouts/AppLayout.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
{
      path: '/',
      component: AppLayout, // O Layout é o componente da rota pai
      children: [ // As páginas são "filhas" do layout
        {
          path: '', // Rota raiz (ex: dashboard)
          name: 'home',
          component: HomeView
        },
        // ... outras rotas que usam o mesmo layout
      ]
    },
    // {
    //   path: '/login', // Uma rota que NÃO usa o AppLayout
    //   name: 'login',
    //   component: () => import('../views/LoginView.vue')
    // }
  ],
})

export default router
