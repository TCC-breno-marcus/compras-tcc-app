/**
 * Compara um objeto de formulário (parcial) com um objeto original completo.
 * Retorna 'true' se qualquer valor nas chaves do formulário for diferente do original.
 * @param origin O objeto original completo.
 * @param edited O objeto com os dados do formulário (parcial).
 */
export function dataHasBeenChanged<T extends object>(
  origin: T | Partial<T> | null,
  edited: Partial<T>,
): boolean {
  if (!origin) {
    return false
  }

  for (const key in edited) {
    if (Object.prototype.hasOwnProperty.call(origin, key)) {
      const typedKey = key as keyof T

      const originValue = origin[typedKey]
      const editedValue = edited[typedKey]

      if (typeof originValue === 'object' && originValue !== null) {
        if (JSON.stringify(originValue) !== JSON.stringify(editedValue)) {
          return true
        }
      } else {
        if (originValue !== editedValue) {
          return true
        }
      }
    }
  }

  return false
}
