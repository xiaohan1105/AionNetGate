import http from './http'
import type { ApiResponse, PagedResponse } from './types'

export interface BlacklistEntry {
  id: number
  ipAddress: string
  reason?: string
  isPermanent: boolean
  createdAt: string
  expiresAt?: string
  isActive: boolean
}

export interface AddBlacklistRequest {
  ipAddress: string
  reason?: string
  duration?: number // 分钟，null表示永久
}

export interface BlacklistQuery {
  page?: number
  pageSize?: number
  search?: string
  isActive?: boolean
}

export const blacklistApi = {
  /**
   * 获取黑名单列表
   */
  getList(query: BlacklistQuery = {}): Promise<ApiResponse<PagedResponse<BlacklistEntry>>> {
    return http.get('/blacklist', { params: query })
  },

  /**
   * 添加IP到黑名单
   */
  add(data: AddBlacklistRequest): Promise<ApiResponse<null>> {
    return http.post('/blacklist', data)
  },

  /**
   * 从黑名单移除
   */
  remove(id: number): Promise<ApiResponse<null>> {
    return http.delete(`/blacklist/${id}`)
  },

  /**
   * 清理过期记录
   */
  cleanup(): Promise<ApiResponse<null>> {
    return http.post('/blacklist/cleanup')
  }
}
