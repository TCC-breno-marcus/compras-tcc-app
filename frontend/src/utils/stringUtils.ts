/**
 * Normaliza o texto para formato de título.
 * @param str Texto de entrada.
 * @returns Texto com cada palavra iniciando em maiúscula.
 */
export const toTitleCase = (str: string): string => {
  if (!str) return ''
  return str.toLowerCase().replace(/(?:^|\s)\w/g, (match) => match.toUpperCase())
}
