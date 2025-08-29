import { defineStore } from 'pinia'
import { authService } from '@/features/autentication/services/authService'
import type { UserCredentials, UserData, UserRegistration } from '@/features/autentication/types'
import { isTokenExpired } from '@/utils/jwtHelper'
import { computed, ref } from 'vue'
import { useMySolicitationListStore } from '@/features/solicitations/stores/mySolicitationList'
import { useSolicitationStore } from '@/features/solicitations/stores/solicitationStore'
import { useSolicitationCartStore } from '@/features/solicitations/stores/solicitationCartStore'

export const useAuthStore = defineStore(
  'auth',
  () => {
    const user = ref<UserData | null>(null)
    const token = ref<string | null>(null)
    const departamentos = ref<string[]>([])

    const isAuthenticated = computed(() => !!token.value && !isTokenExpired(token.value))
    const isAdmin = computed(() => user.value?.role === 'Admin')
    const isGestor = computed(() => user.value?.role === 'Gestor')
    const isSolicitante = computed(() => user.value?.role === 'Solicitante')

    const login = async (credentials: UserCredentials) => {
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

    const fetchDataUser = async () => {
      try {
        const myData = await authService.getMyData()
        user.value = myData
      } catch (error) {
        console.error('Falha na ação de fetchDataUser da store:', error)
      }
    }

    const register = async (userData: UserRegistration) => {
      try {
        await authService.register(userData)
        return true
      } catch (error) {
        console.error('Falha na ação de registrar da store:', error)
        return false
      }
    }

    const logout = () => {
      useMySolicitationListStore().$reset()
      useSolicitationStore().$reset()
      useSolicitationCartStore().$reset()
      user.value = null
      token.value = null
    }

    const fetchDeptos = async () => {
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
      isSolicitante,
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
