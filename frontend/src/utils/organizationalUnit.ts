import type { UnitOrganizational } from '@/features/unitOrganizational/types'
import { toTitleCase } from './stringUtils'

/**
 * Formata unidade organizacional para exibição em três modos.
 * @param unit Unidade organizacional de origem.
 * @param mode Define se retorna apenas nome, sigla ou ambos.
 * @returns Texto formatado pronto para UI.
 */
export const unitOrganizationalFormatString = (
  unit: UnitOrganizational| null | undefined,
  mode: 'name' | 'acronym' | 'full' = 'full',
) => {
  if (!unit) {
    return 'Não se aplica'
  }

  const { nome, sigla } = unit

  if (mode === 'name') {
    return toTitleCase(nome)
  }

  if (mode === 'acronym') {
    return sigla.toUpperCase()
  }

  return `${toTitleCase(nome)} (${sigla.toUpperCase()})`
}
