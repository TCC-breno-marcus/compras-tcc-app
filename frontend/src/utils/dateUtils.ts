type DateFormatType = 'short' | 'long'

/**
 * Formata um objeto Date para uma string legível.
 * @param date O objeto Date a ser formatado.
 * @param formatType O tipo de formato: 'short' (dd/mm/aaaa) ou 'long' (data por extenso com hora).
 * @returns A data formatada como string.
 */
export const formatDate = (
  date: Date | string | null,
  formatType: DateFormatType = 'short',
): string => {
  if (!date) {
    return ''
  }

  const validDate = new Date(date)

  if (formatType === 'long') {
    return validDate.toLocaleString('pt-BR', {
      dateStyle: 'long',
      timeStyle: 'short',
    })
  }

  return validDate.toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  })
}
