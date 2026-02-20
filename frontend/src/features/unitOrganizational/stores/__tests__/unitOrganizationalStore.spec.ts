import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

const { getCentersMock, getDepartmentsMock } = vi.hoisted(() => ({
  getCentersMock: vi.fn(),
  getDepartmentsMock: vi.fn(),
}))

vi.mock('../../services/unitOrganizationalService', () => ({
  unitOrganizationalService: {
    getCenters: getCentersMock,
    getDepartments: getDepartmentsMock,
  },
}))

import { useUnitOrganizationalStore } from '../unitOrganizationalStore'

describe('Store: unitOrganizationalStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('deve buscar e preencher centros', async () => {
    const store = useUnitOrganizationalStore()
    getCentersMock.mockResolvedValue([{ id: 1, nome: 'Centro A' }])

    await store.fetchCenters()

    expect(store.centers).toEqual([{ id: 1, nome: 'Centro A' }])
  })

  it('deve buscar e preencher departamentos', async () => {
    const store = useUnitOrganizationalStore()
    getDepartmentsMock.mockResolvedValue([{ id: 2, nome: 'Dep A' }])

    await store.fetchDepartments()

    expect(store.departments).toEqual([{ id: 2, nome: 'Dep A' }])
  })

  it('deve manter estado atual quando serviço falhar', async () => {
    const store = useUnitOrganizationalStore()
    store.centers = [{ id: 1, nome: 'Centro A' } as any]
    getCentersMock.mockRejectedValue(new Error('erro'))

    await store.fetchCenters()

    expect(store.centers).toEqual([{ id: 1, nome: 'Centro A' }])
  })
})

