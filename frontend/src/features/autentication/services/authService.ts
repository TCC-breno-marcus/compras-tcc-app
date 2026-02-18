import { apiClient } from '@/services/apiClient'
import type { UserCredentials, UserRegistration, AuthResponse } from '../types'
import axios from 'axios'
import type { User } from '@/features/users/types'

export const authService = {
  /**
   * Autentica usuário e retorna token + dados básicos de sessão.
   * @param credentials Credenciais de acesso.
   * @returns Payload de autenticação.
   */
  async login(credentials: UserCredentials): Promise<AuthResponse> {
    try {
      const response = await apiClient.post('/auth/login', credentials)
      return response.data
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        throw new Error(error.response.data.message || 'Credenciais inválidas.')
      }
      throw new Error('Erro de conexão com a API ao tentar logar no sistema.')
    }
  },
  /**
   * Registra um novo usuário no backend.
   * @param userData Dados cadastrais do usuário.
   */
  async register(userData: UserRegistration): Promise<void> {
    try {
      await apiClient.post('/auth/register', userData)
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        throw new Error(error.response.data.message)
      }
      throw new Error('Erro de conexão com a API ao tentar registrar usuário.')
    }
  },
  /**
   * Recupera dados do usuário autenticado.
   * @returns Perfil do usuário logado.
   */
  async getMyData(): Promise<User> {
    try {
      const response = await apiClient.get('/auth/me')
      return response.data
    } catch (error) {
      if (axios.isAxiosError(error) && error.response) {
        throw new Error(error.response.data.message)
      }
      throw new Error('Erro de conexão com a API ao tentar buscar dados do usuário.')
    }
  },
}
