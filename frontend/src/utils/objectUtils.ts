import type { Item } from '@/features/management/types'

/**
 * Compara um objeto de formulário (parcial) com um objeto original completo.
 * Retorna 'true' se qualquer valor nas chaves do formulário for diferente do original.
 * @param origin O objeto original completo (ex: detailedItem).
 * @param edited O objeto com os dados do formulário (ex: formData).
 */
export function dataHasBeenChanged(origin: Item | null, edited: Partial<Item>): boolean {
  if (!origin) {
    return false
  }

  for (const key in edited) {
    if (Object.prototype.hasOwnProperty.call(origin, key)) {
      const typedKey = key as keyof Item

      if (edited[typedKey] !== origin[typedKey]) {
        console.log(
          `Diferença encontrada na chave '${typedKey}': Original='${origin[typedKey]}', Editado='${edited[typedKey]}'`,
        )
        return true
      }
    }
  }

  return false
}
