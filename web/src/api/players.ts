import http from './http'
import type { ApiResponse, PagedResponse, Player, PlayerDetail } from './types'

export interface PlayerQuery {
  page?: number
  pageSize?: number
  search?: string
  status?: number
  orderBy?: string
  desc?: boolean
}

export const playersApi = {
  getPlayers(query: PlayerQuery = {}): Promise<ApiResponse<PagedResponse<Player>>> {
    return http.get('/players', { params: query })
  },

  getPlayer(id: number): Promise<ApiResponse<PlayerDetail>> {
    return http.get(`/players/${id}`)
  },

  updateStatus(id: number, status: number): Promise<ApiResponse> {
    return http.patch(`/players/${id}/status`, { status })
  },

  updateRole(id: number, role: number): Promise<ApiResponse> {
    return http.patch(`/players/${id}/role`, { role })
  },

  unlockAccount(id: number): Promise<ApiResponse> {
    return http.post(`/players/${id}/unlock`)
  },

  kickPlayer(id: number): Promise<ApiResponse> {
    return http.post(`/players/${id}/kick`)
  },

  resetPassword(id: number): Promise<ApiResponse<string>> {
    return http.post(`/players/${id}/reset-password`)
  }
}
