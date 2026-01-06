import http from './http'
import type { ApiResponse, DashboardStats, OnlineTrendPoint, ActivityLog } from './types'

export const dashboardApi = {
  getStats(): Promise<ApiResponse<DashboardStats>> {
    return http.get('/dashboard/stats')
  },

  getOnlineTrend(hours: number = 24): Promise<ApiResponse<OnlineTrendPoint[]>> {
    return http.get('/dashboard/online-trend', { params: { hours } })
  },

  getRevenue(period: string = 'week'): Promise<ApiResponse<any>> {
    return http.get('/dashboard/revenue', { params: { period } })
  },

  getActivities(limit: number = 20): Promise<ApiResponse<ActivityLog[]>> {
    return http.get('/dashboard/activities', { params: { limit } })
  }
}
