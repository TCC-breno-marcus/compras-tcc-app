import { beforeEach, describe, expect, it, vi } from 'vitest'

const authState = {
  isAuthenticated: false,
  user: null as null | { role: string },
}

const cartResetMock = vi.fn(() => {
  cartState.solicitationItems = []
  cartState.solicitationType = null
})
const confirmRequireMock = vi.fn()

const cartState = {
  solicitationItems: [] as Array<{ id: number }>,
  solicitationType: null as null | 'geral' | 'patrimonial',
  $reset: cartResetMock,
}

vi.mock('@/features/autentication/stores/authStore', () => ({
  useAuthStore: () => authState,
}))

vi.mock('@/features/solicitations/stores/solicitationCartStore', () => ({
  useSolicitationCartStore: () => cartState,
}))

vi.mock('primevue', () => ({
  useConfirm: () => ({
    require: confirmRequireMock,
  }),
}))

import router from '../index'

describe('Router Guard', () => {
  beforeEach(async () => {
    vi.clearAllMocks()
    authState.isAuthenticated = false
    authState.user = null
    cartState.solicitationItems = []
    cartState.solicitationType = null
    await router.push('/login')
  })

  it('deve redirecionar para login ao acessar rota protegida sem autenticação', async () => {
    await router.push('/solicitacoes')

    expect(router.currentRoute.value.name).toBe('Login')
    expect(router.currentRoute.value.query.redirect).toBe('/solicitacoes')
  })

  it('deve redirecionar para unauthorized quando role não permitida', async () => {
    authState.isAuthenticated = true
    authState.user = { role: 'Solicitante' }

    await router.push('/gestor/dashboard')

    expect(router.currentRoute.value.name).toBe('Unauthorized')
  })

  it('deve pedir confirmação ao trocar tipo de solicitação com carrinho preenchido', async () => {
    const pushSpy = vi.spyOn(router, 'push')
    authState.isAuthenticated = true
    authState.user = { role: 'Solicitante' }
    cartState.solicitationItems = [{ id: 1 }]
    cartState.solicitationType = 'geral'

    await router.push('/solicitacoes/criar/geral')
    await router.push('/solicitacoes/criar/patrimonial')

    expect(confirmRequireMock).toHaveBeenCalledTimes(1)
    expect(router.currentRoute.value.path).toBe('/solicitacoes/criar/geral')

    const args = confirmRequireMock.mock.calls[0][0]
    await args.accept()

    expect(cartResetMock).toHaveBeenCalledTimes(1)
    expect(pushSpy).toHaveBeenCalledWith('/solicitacoes/criar/patrimonial')
  })
})
