import axios from 'axios'
import { useAuthStore } from '@/features/autentication/stores/authStore'

const apiUrl = import.meta.env.VITE_API_URL || '/api'

/**
 * Instância compartilhada de cliente HTTP da aplicação.
 */
export const apiClient = axios.create({
  baseURL: apiUrl,
  headers: {
    'Content-Type': 'application/json',
  },
})

apiClient.interceptors.request.use(
  /**
   * Injeta token JWT atual em todas as requisições autenticadas.
   */
  (config) => {
    const authStore = useAuthStore()
    const token = authStore.token

    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }

    return config
  },
  (error) => {
    return Promise.reject(error)
  },
)
