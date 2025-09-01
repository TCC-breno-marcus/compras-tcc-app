/**
 * Compara um objeto de formulário (parcial) com um objeto original completo.
 * Retorna 'true' se qualquer valor nas chaves do formulário for diferente do original.
 * @param origin O objeto original completo.
 * @param edited O objeto com os dados do formulário (parcial).
 */
export const dataHasBeenChanged = <T extends object>(
  origin: T | Partial<T> | null,
  edited: Partial<T>,
): boolean => {
  return getChangedProperties(origin, edited).length > 0
}

/**
 * Compara dois objetos e retorna um array com as chaves das propriedades que mudaram.
 * @param origin O objeto original completo.
 * @param edited O objeto com os dados do formulário (parcial).
 * @returns Um array com os nomes das chaves que foram alteradas.
 */
export const getChangedProperties = <T extends object>(
  origin: T | Partial<T> | null,
  edited: Partial<T>,
): (keyof T)[] => {
  const changedKeys: (keyof T)[] = []

  if (!origin) {
    return Object.keys(edited) as (keyof T)[]
  }

  for (const key in edited) {
    const typedKey = key as keyof T
    if (!Object.prototype.hasOwnProperty.call(origin, typedKey)) {
      changedKeys.push(typedKey)
      continue
    }

    const originValue = origin[typedKey]
    const editedValue = edited[typedKey]

    let isDifferent = false

    if (typeof originValue === 'object' && originValue !== null) {
      if (JSON.stringify(originValue) !== JSON.stringify(editedValue)) {
        isDifferent = true
      }
    } else {
      if (originValue !== editedValue) {
        isDifferent = true
      }
    }

    if (isDifferent) {
      changedKeys.push(typedKey)
    }
  }

  return changedKeys
}
