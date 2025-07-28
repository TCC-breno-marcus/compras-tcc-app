/**
 * Valida se uma string está em um formato de email válido.
 * @param email O email a ser validado.
 * @returns 'true' se o email for válido, 'false' caso contrário.
 */
export function emailValidator(email: string): boolean {
  if (!email) {
    return false
  }

  const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/

  return emailRegex.test(email)
}

/**
 * Valida um número de CPF brasileiro.
 * @param cpf O CPF a ser validado (pode conter pontos e traço).
 * @returns 'true' se o CPF for válido, 'false' caso contrário.
 */
export function cpfValidator(cpf: string): boolean {
  if (!cpf) {
    return false
  }

  // Remove caracteres não numéricos
  const cpfLimpo = cpf.replace(/\D/g, '')

  // Verifica se o CPF tem 11 dígitos
  if (cpfLimpo.length !== 11) {
    return false
  }

  // Verifica se todos os dígitos são iguais (ex: 111.111.111-11), o que é inválido
  if (/^(\d)\1{10}$/.test(cpfLimpo)) {
    return false
  }

  // Calcula o primeiro dígito verificador
  let soma = 0
  for (let i = 0; i < 9; i++) {
    soma += parseInt(cpfLimpo.charAt(i)) * (10 - i)
  }
  let resto = 11 - (soma % 11)
  let digitoVerificador1 = resto === 10 || resto === 11 ? 0 : resto

  if (digitoVerificador1 !== parseInt(cpfLimpo.charAt(9))) {
    return false
  }

  // Calcula o segundo dígito verificador
  soma = 0
  for (let i = 0; i < 10; i++) {
    soma += parseInt(cpfLimpo.charAt(i)) * (11 - i)
  }
  resto = 11 - (soma % 11)
  let digitoVerificador2 = resto === 10 || resto === 11 ? 0 : resto

  if (digitoVerificador2 !== parseInt(cpfLimpo.charAt(10))) {
    return false
  }

  return true
}
