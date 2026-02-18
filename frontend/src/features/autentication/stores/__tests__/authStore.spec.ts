import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const {
  authServiceMock,
  mySolicitationResetMock,
  solicitationResetMock,
  cartResetMock,
  fetchSettingsMock,
} = vi.hoisted(() => ({
  authServiceMock: {
    login: vi.fn(),
    getMyData: vi.fn(),
    register: vi.fn(),
  },
  mySolicitationResetMock: vi.fn(),
  solicitationResetMock: vi.fn(),
  cartResetMock: vi.fn(),
  fetchSettingsMock: vi.fn(),
}))

vi.mock('@/features/autentication/services/authService', () => ({
  authService: authServiceMock,
}))

vi.mock('@/features/solicitations/stores/mySolicitationList', () => ({
  useMySolicitationListStore: () => ({
    $reset: mySolicitationResetMock,
  }),
}))

vi.mock('@/features/solicitations/stores/solicitationStore', () => ({
  useSolicitationStore: () => ({
    $reset: solicitationResetMock,
  }),
}))

vi.mock('@/features/solicitations/stores/solicitationCartStore', () => ({
  useSolicitationCartStore: () => ({
    $reset: cartResetMock,
  }),
}))

vi.mock('@/features/settings/stores/settingStore', () => ({
  useSettingStore: () => ({
    fetchSettings: fetchSettingsMock,
  }),
}))

vi.mock('@/utils/jwtHelper', () => ({
  isTokenExpired: vi.fn(() => false),
}))

import { useAuthStore } from '../authStore'

describe('Store: authStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('deve realizar login com sucesso e carregar configurações', async () => {
    const store = useAuthStore()
    authServiceMock.login.mockResolvedValue({ token: 'token-123' })
    fetchSettingsMock.mockResolvedValue(undefined)

    await store.login({ email: 'gestor@sistema.com', password: '123456' })

    expect(store.token).toBe('token-123')
    expect(fetchSettingsMock).toHaveBeenCalledTimes(1)
  })

  it('deve fazer logout quando login falhar', async () => {
    const store = useAuthStore()
    authServiceMock.login.mockRejectedValue(new Error('Falha login'))

    await expect(
      store.login({ email: 'gestor@sistema.com', password: '123456' }),
    ).rejects.toBe('Falha login')

    expect(store.token).toBeNull()
    expect(store.user).toBeNull()
    expect(mySolicitationResetMock).toHaveBeenCalledTimes(1)
    expect(solicitationResetMock).toHaveBeenCalledTimes(1)
    expect(cartResetMock).toHaveBeenCalledTimes(1)
  })

  it('deve buscar dados do usuário autenticado', async () => {
    const store = useAuthStore()
    const mockUser = { id: '1', nome: 'Gestor', email: 'gestor@sistema.com', role: 'Gestor' }
    authServiceMock.getMyData.mockResolvedValue(mockUser)

    await store.fetchDataUser()

    expect(store.user).toEqual(mockUser)
  })

  it('deve propagar erro no registro', async () => {
    const store = useAuthStore()
    authServiceMock.register.mockRejectedValue(new Error('Erro ao registrar'))

    await expect(
      store.register({
        nome: 'Novo',
        email: 'novo@sistema.com',
        telefone: '99999999',
        cpf: '12345678901',
        password: '123456',
        role: 'Solicitante',
      }),
    ).rejects.toBe('Erro ao registrar')
  })

  it('deve resetar estado e stores dependentes no logout', () => {
    const store = useAuthStore()
    store.user = { id: '1', nome: 'Gestor', email: 'gestor@sistema.com', role: 'Gestor' } as any
    store.token = 'token-123'

    store.logout()

    expect(store.user).toBeNull()
    expect(store.token).toBeNull()
    expect(mySolicitationResetMock).toHaveBeenCalledTimes(1)
    expect(solicitationResetMock).toHaveBeenCalledTimes(1)
    expect(cartResetMock).toHaveBeenCalledTimes(1)
  })

  it('deve calcular flags de perfil corretamente', () => {
    const store = useAuthStore()
    store.token = 'token-123'
    store.user = { id: '1', nome: 'Admin', email: 'admin@sistema.com', role: 'Admin' } as any

    expect(store.isAuthenticated).toBe(true)
    expect(store.isAdmin).toBe(true)
    expect(store.isGestor).toBe(false)
    expect(store.isSolicitante).toBe(false)
  })
})
