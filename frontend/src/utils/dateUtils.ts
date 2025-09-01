type DateFormatType = 'short' | 'long' | 'iso'

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

  if (formatType === 'iso') {
    return validDate.toISOString().split('T')[0]
  }

  return validDate.toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  })
}

export function parseDateString(dateString: string | null): Date | null {
  if (!dateString || dateString.trim() === '') {
    return null
  }

  try {
    if (dateString.includes('T')) {
      const date = new Date(dateString)
      if (!isNaN(date.getTime())) {
        return date
      }
    }

    if (dateString.includes('-')) {
      return new Date(`${dateString}T00:00:00`)
    }

    if (dateString.includes(' de ')) {
      const monthMap: { [key: string]: number } = {
        janeiro: 0,
        fevereiro: 1,
        março: 2,
        abril: 3,
        maio: 4,
        junho: 5,
        julho: 6,
        agosto: 7,
        setembro: 8,
        outubro: 9,
        novembro: 10,
        dezembro: 11,
      }
      const parts = dateString.toLowerCase().split(' às ')
      const datePart = parts[0].split(' de ')
      const timePart = parts[1] ? parts[1].split(':') : ['0', '0']
      const day = parseInt(datePart[0], 10)
      const month = monthMap[datePart[1]]
      const year = parseInt(datePart[2], 10)
      const hours = parseInt(timePart[0], 10)
      const minutes = parseInt(timePart[1], 10)
      if (!isNaN(day) && month !== undefined && !isNaN(year)) {
        return new Date(year, month, day, hours || 0, minutes || 0)
      }
    }

    const shortParts = dateString.split('/')
    if (shortParts.length === 3) {
      const day = parseInt(shortParts[0], 10)
      const month = parseInt(shortParts[1], 10) - 1
      const year = parseInt(shortParts[2], 10)
      if (!isNaN(day) && !isNaN(month) && !isNaN(year)) {
        const date = new Date(year, month, day)
        if (date.getFullYear() === year && date.getMonth() === month && date.getDate() === day) {
          return date
        }
      }
    }

    return null
  } catch (error) {
    console.error('Erro ao parsear a data:', dateString, error)
    return null
  }
}
