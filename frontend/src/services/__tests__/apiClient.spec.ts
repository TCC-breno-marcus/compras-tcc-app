import { describe, it, expect, vi, beforeEach } from 'vitest'

const authState = {
  token: null as string | null,
}

vi.mock('@/features/autentication/stores/authStore', () => ({
  useAuthStore: () => authState,
}))

import { apiClient } from '@/services/apiClient'

describe('Service: apiClient', () => {
  beforeEach(() => {
    authState.token = null
  })

  it('deve usar "/api" como baseURL padrão', () => {
    expect(apiClient.defaults.baseURL).toBe('/api')
  })

  it('deve incluir Authorization quando houver token', async () => {
    authState.token = 'jwt-token'

    const handler = (apiClient.interceptors.request as any).handlers[0].fulfilled
    const config = await handler({ headers: {} })

    expect(config.headers.Authorization).toBe('Bearer jwt-token')
  })

  it('não deve incluir Authorization quando não houver token', async () => {
    const handler = (apiClient.interceptors.request as any).handlers[0].fulfilled
    const config = await handler({ headers: {} })

    expect(config.headers.Authorization).toBeUndefined()
  })
})

