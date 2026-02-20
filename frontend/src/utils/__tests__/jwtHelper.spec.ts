// isTokenExpired.test.ts
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { isTokenExpired } from '../jwtHelper'
import { jwtDecode } from 'jwt-decode'

vi.mock('jwt-decode')

describe('isTokenExpired', () => {
  const mockJwtDecode = vi.mocked(jwtDecode)
  const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {})

  beforeEach(() => {
    vi.clearAllMocks()
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2024-01-01T12:00:00Z'))
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it('deve retornar true quando o token é null', () => {
    expect(isTokenExpired(null)).toBe(true)
  })

  it('deve retornar true quando o token é uma string vazia', () => {
    expect(isTokenExpired('')).toBe(true)
  })

  it('deve retornar true quando o token está expirado', () => {
    const nowInSeconds = Math.floor(Date.now() / 1000)
    const expiredToken = { exp: nowInSeconds - 3600 } // Expirado há 1 hora

    mockJwtDecode.mockReturnValue(expiredToken)

    expect(isTokenExpired('expired.token.here')).toBe(true)
    expect(mockJwtDecode).toHaveBeenCalledWith('expired.token.here')
  })

  it('deve retornar false quando o token ainda é válido', () => {
    const nowInSeconds = Math.floor(Date.now() / 1000)
    const validToken = { exp: nowInSeconds + 3600 } // Expira em 1 hora

    mockJwtDecode.mockReturnValue(validToken)

    expect(isTokenExpired('valid.token.here')).toBe(false)
    expect(mockJwtDecode).toHaveBeenCalledWith('valid.token.here')
  })

  it('deve retornar false quando o token expira exatamente no segundo atual', () => {
    const nowInSeconds = Math.floor(Date.now() / 1000)
    const tokenExpiringNow = { exp: nowInSeconds }

    mockJwtDecode.mockReturnValue(tokenExpiringNow)

    expect(isTokenExpired('token.expiring.now')).toBe(false)
  })

  it('deve retornar true quando o token expira um segundo antes do atual', () => {
    const nowInSeconds = Math.floor(Date.now() / 1000)
    const expiredByOneSecond = { exp: nowInSeconds - 1 }

    mockJwtDecode.mockReturnValue(expiredByOneSecond)

    expect(isTokenExpired('token.expired.one.second')).toBe(true)
  })

  it('deve retornar true quando jwtDecode lança um erro', () => {
    mockJwtDecode.mockImplementation(() => {
      throw new Error('Token inválido')
    })

    expect(isTokenExpired('invalid.token')).toBe(true)
    expect(consoleSpy).toHaveBeenCalledWith(
      'Erro ao decodificar o token:',
      expect.any(Error)
    )
  })

  it('deve retornar true quando jwtDecode lança um erro genérico', () => {
    mockJwtDecode.mockImplementation(() => {
      throw new Error('Unexpected error')
    })

    expect(isTokenExpired('malformed.token')).toBe(true)
    expect(consoleSpy).toHaveBeenCalled()
  })
})