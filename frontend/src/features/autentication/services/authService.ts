import { apiClient } from '@/services/apiClient'
import type { UserCredentials, UserRegistration, AuthResponse } from '../types'
import axios from 'axios'

export const authService = {
  async login(credentials: UserCredentials): Promise<AuthResponse> {
    try {
      const response = await apiClient.post('/auth/login', credentials)
      return response.data
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        throw new Error(error.response.data.message || 'Credenciais inválidas.')
      }
      throw new Error('Erro de conexão com a API.')
    }
  },
  async register(userData: UserRegistration): Promise<void> {
    try {
      await apiClient.post('/auth/register', userData)
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        throw new Error(error.response.data.message)
      }
      throw new Error('Erro de conexão com a API.')
    }
  },
}
