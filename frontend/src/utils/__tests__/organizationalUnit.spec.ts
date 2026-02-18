// unitOrganizationalFormatString.test.ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { unitOrganizationalFormatString } from '../organizationalUnit'
import type { UnitOrganizational } from '@/features/unitOrganizational/types'
import * as stringUtils from '../stringUtils'

vi.mock('../stringUtils', () => ({
  toTitleCase: vi.fn((str: string) => {
    if (!str) return ''
    return str.toLowerCase().replace(/(?:^|\s)\w/g, (match) => match.toUpperCase())
  }),
}))

describe('unitOrganizationalFormatString', () => {
  const mockUnit: UnitOrganizational = {
    nome: 'DEPARTAMENTO DE TECNOLOGIA',
    sigla: 'dti',
  } as UnitOrganizational

  let toTitleCaseMock: ReturnType<typeof vi.fn>

  beforeEach(async () => {
    vi.clearAllMocks()
    const stringUtils = await import('../stringUtils')
    toTitleCaseMock = stringUtils.toTitleCase as ReturnType<typeof vi.fn>
  })

  it('deve retornar "Não se aplica" quando unit é null', () => {
    expect(unitOrganizationalFormatString(null)).toBe('Não se aplica')
  })

  it('deve retornar "Não se aplica" quando unit é undefined', () => {
    expect(unitOrganizationalFormatString(undefined)).toBe('Não se aplica')
  })

  it('deve formatar com mode "full" por padrão', () => {
    const result = unitOrganizationalFormatString(mockUnit)

    expect(stringUtils.toTitleCase).toHaveBeenCalledWith('DEPARTAMENTO DE TECNOLOGIA')
    expect(result).toBe('Departamento De Tecnologia (DTI)')
  })

  it('deve formatar com mode "full" quando especificado', () => {
    const result = unitOrganizationalFormatString(mockUnit, 'full')

    expect(result).toBe('Departamento De Tecnologia (DTI)')
  })

  it('deve formatar com mode "name"', () => {
    const result = unitOrganizationalFormatString(mockUnit, 'name')

    expect(stringUtils.toTitleCase).toHaveBeenCalledWith('DEPARTAMENTO DE TECNOLOGIA')
    expect(result).toBe('Departamento De Tecnologia')
  })

  it('deve formatar com mode "acronym"', () => {
    const result = unitOrganizationalFormatString(mockUnit, 'acronym')

    expect(result).toBe('DTI')
  })

  it('deve converter sigla para maiúsculas no mode "acronym"', () => {
    const unitLowerCase: UnitOrganizational = {
      nome: 'Departamento',
      sigla: 'dept',
    } as UnitOrganizational

    expect(unitOrganizationalFormatString(unitLowerCase, 'acronym')).toBe('DEPT')
  })

  it('deve converter sigla para maiúsculas no mode "full"', () => {
    const unitMixedCase: UnitOrganizational = {
      nome: 'Departamento de RH',
      sigla: 'RhDept',
    } as UnitOrganizational

    const result = unitOrganizationalFormatString(unitMixedCase, 'full')
    expect(result).toContain('RHDEPT')
  })

  it('deve lidar com nomes vazios', () => {
    const emptyNameUnit: UnitOrganizational = {
      nome: '',
      sigla: 'TST',
    } as UnitOrganizational

    expect(unitOrganizationalFormatString(emptyNameUnit, 'name')).toBe('')
    expect(unitOrganizationalFormatString(emptyNameUnit, 'full')).toBe(' (TST)')
  })

  it('deve lidar com siglas vazias', () => {
    const emptyAcronymUnit: UnitOrganizational = {
      nome: 'Departamento Teste',
      sigla: '',
    } as UnitOrganizational

    expect(unitOrganizationalFormatString(emptyAcronymUnit, 'acronym')).toBe('')
    expect(unitOrganizationalFormatString(emptyAcronymUnit, 'full')).toContain('()')
  })
})
