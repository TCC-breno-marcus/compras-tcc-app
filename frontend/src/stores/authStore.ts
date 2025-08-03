import { defineStore } from 'pinia'
import { authService } from '@/features/auth/services/authService'
import type { UserCredentials, UserRegistration } from '@/features/auth/types'
import { isTokenExpired } from '@/utils/jwtHelper';

interface AuthState {
  user: { id: number; name: string; email: string; role: string } | null
  token: string | null
}

export const useAuthStore = defineStore('auth', {
  state: (): AuthState => ({
    user: null,
    token: null,
  }),
  getters: {
    isAuthenticated: (state): boolean => {
      return !!state.token && !isTokenExpired(state.token)
    },
    isAdmin: (state) => state.user?.role === 'Admin',
    isGestor: (state) => state.user?.role === 'Gestor',
  },
  actions: {
    async login(credentials: UserCredentials) {
      try {
        const { token } = await authService.login(credentials)
        //TODO: deve pegar os dados do user no backend
        const userData = {
          id: 2,
          name: 'Admin do Sistema',
          email: 'admin@sistema.com',
          telefone: '99999999999',
          role: 'Admin',
        }
        this.user = userData
        this.token = token
        return true
      } catch (error) {
        console.error('Falha na ação de login da store:', error)
        this.logout()
        return false
      }
    },
    async register(userData: UserRegistration) {
      await authService.register(userData)
    },
    logout() {
      this.user = null
      this.token = null
    },
  },
  persist: true,
})
