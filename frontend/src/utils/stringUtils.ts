export function toTitleCase(str: string): string {
  if (!str) return '';
  // Converte a string para minúsculas e depois capitaliza a primeira letra de cada palavra
  return str.toLowerCase().replace(/(?:^|\s)\w/g, (match) => match.toUpperCase());
}