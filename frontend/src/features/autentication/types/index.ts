/**
 * O formato dos dados que o usuário envia para fazer login.
 */
export interface UserCredentials {
  email: string
  password: string
}

/**
 * O formato dos dados que o usuário envia para se registrar.
 */
export interface UserRegistration {
  nome: string
  email: string
  telefone: string
  cpf: string
  password: string
  departamento: string
}

/**
 * O formato da resposta que a API retorna após um login bem-sucedido.
 */
export interface AuthResponse {
  token: string
  message: string
}

