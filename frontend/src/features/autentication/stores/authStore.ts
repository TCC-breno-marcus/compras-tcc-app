import { defineStore } from 'pinia'
import { authService } from '@/features/autentication/services/authService'
import type { UserCredentials, UserData, UserRegistration } from '@/features/autentication/types'
import { isTokenExpired } from '@/utils/jwtHelper'
import { computed, ref } from 'vue'

export const useAuthStore = defineStore(
  'auth',
  () => {
    const user = ref<UserData | null>(null)
    const token = ref<string | null>(null)
    const departamentos = ref<string[]>([])

    const isAuthenticated = computed(() => !!token.value && !isTokenExpired(token.value))
    const isAdmin = computed(() => user.value?.role === 'Admin')
    const isGestor = computed(() => user.value?.role === 'Gestor')

    async function login(credentials: UserCredentials) {
      try {
        const response = await authService.login(credentials)
        token.value = response.token
        return true
      } catch (error) {
        console.error('Falha na ação de login da store:', error)
        logout()
        return false
      }
    }

    async function fetchDataUser() {
      try {
        const myData = await authService.getMyData()
        user.value = myData
      } catch (error) {
        console.error('Falha na ação de fetchDataUser da store:', error)
      }
    }

    async function register(userData: UserRegistration) {
      try {
        await authService.register(userData)
        return true
      } catch (error) {
        console.error('Falha na ação de registrar da store:', error)
        return false
      }
    }

    function logout() {
      user.value = null
      token.value = null
    }

    async function fetchDeptos() {
      try {
        const data = await authService.getDeptos()
        departamentos.value = data
      } catch (error) {
        console.error('Falha na ação de fetchDataUser da store:', error)
      }
    }

    return {
      user,
      token,
      departamentos,
      isAuthenticated,
      isAdmin,
      isGestor,
      login,
      fetchDataUser,
      register,
      logout,
      fetchDeptos,
    }
  },
  {
    persist: true,
  },
)
