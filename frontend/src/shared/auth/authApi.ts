import { request } from '../api/httpClient'
import type { AuthSession } from './authSession'

interface Credentials {
  email: string
  password: string
}

export const authApi = {
  register(credentials: Credentials): Promise<AuthSession> {
    return request<AuthSession>('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify(credentials),
    })
  },

  login(credentials: Credentials): Promise<AuthSession> {
    return request<AuthSession>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify(credentials),
    })
  },
}
