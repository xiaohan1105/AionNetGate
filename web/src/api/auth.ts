import http from './http'
import type { ApiResponse, LoginResponse, UserInfo } from './types'

export const authApi = {
  login(username: string, password: string): Promise<ApiResponse<LoginResponse>> {
    return http.post('/auth/login', { username, password })
  },

  register(username: string, password: string, email?: string): Promise<ApiResponse<UserInfo>> {
    return http.post('/auth/register', { username, password, email })
  },

  logout(): Promise<ApiResponse> {
    return http.post('/auth/logout')
  },

  refresh(refreshToken: string): Promise<ApiResponse<LoginResponse>> {
    return http.post('/auth/refresh', { refreshToken })
  },

  changePassword(oldPassword: string, newPassword: string): Promise<ApiResponse> {
    return http.post('/auth/change-password', { oldPassword, newPassword })
  },

  me(): Promise<ApiResponse<UserInfo>> {
    return http.get('/auth/me')
  }
}
