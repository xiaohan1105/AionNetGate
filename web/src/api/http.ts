import axios from 'axios'
import type { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios'
import { useAuthStore } from '@/stores/auth'
import router from '@/router'

const http: AxiosInstance = axios.create({
  baseURL: '/api',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// 请求拦截器
http.interceptors.request.use(
  (config) => {
    const authStore = useAuthStore()
    if (authStore.token) {
      config.headers.Authorization = `Bearer ${authStore.token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// 响应拦截器
http.interceptors.response.use(
  (response: AxiosResponse) => {
    return response.data
  },
  async (error) => {
    const authStore = useAuthStore()

    if (error.response?.status === 401) {
      // 尝试刷新 Token
      const refreshed = await authStore.refreshAccessToken()
      if (refreshed) {
        // 重试原请求
        error.config.headers.Authorization = `Bearer ${authStore.token}`
        return http(error.config)
      }

      // 刷新失败，跳转登录
      authStore.clearTokens()
      router.push({ name: 'Login' })
    }

    return Promise.reject(error.response?.data || error)
  }
)

export default http
