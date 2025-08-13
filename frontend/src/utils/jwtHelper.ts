import { jwtDecode } from 'jwt-decode'

/**
 * Verifica se um token JWT está expirado.
 * @param token O token JWT.
 * @returns 'true' se o token estiver expirado, 'false' caso contrário.
 */
export function isTokenExpired(token: string | null): boolean {
  if (!token) {
    return true
  }

  try {
    const decodedToken: { exp: number } = jwtDecode(token)

    const nowInSeconds = Math.floor(Date.now() / 1000)

    return decodedToken.exp < nowInSeconds
  } catch (error) {
    console.error('Erro ao decodificar o token:', error)
    return true
  }
}
