/**
 * Formata um número representando quantidade para string com separador de milhares.
 *
 * @param quantity - Número a ser formatado
 * @returns String formatada no padrão brasileiro (ponto como separador de milhares)
 */
export const formatQuantity = (quantity: number): string => {
  const defaultOptions: Intl.NumberFormatOptions = {
    style: 'decimal',
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
    useGrouping: true,
  }

  return new Intl.NumberFormat('pt-BR', defaultOptions).format(quantity)
}
