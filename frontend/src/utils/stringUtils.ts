// Converte a string para minúsculas e depois capitaliza a primeira letra de cada palavra
export const toTitleCase = (str: string): string => {
  if (!str) return ''
  return str.toLowerCase().replace(/(?:^|\s)\w/g, (match) => match.toUpperCase())
}
