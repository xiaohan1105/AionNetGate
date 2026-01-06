import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi } from '@/api/auth'
import type { UserInfo, LoginResponse } from '@/api/types'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string>(localStorage.getItem('token') || '')
  const refreshToken = ref<string>(localStorage.getItem('refreshToken') || '')
  const user = ref<UserInfo | null>(null)

  const isLoggedIn = computed(() => !!token.value)
  const isAdmin = computed(() => user.value?.role === 99)
  const isGM = computed(() => user.value?.role === 10 || user.value?.role === 99)

  async function login(username: string, password: string) {
    const response = await authApi.login(username, password)
    if (response.success && response.data) {
      setTokens(response.data)
      user.value = response.data.user
    }
    return response
  }

  async function logout() {
    try {
      await authApi.logout()
    } finally {
      clearTokens()
    }
  }

  async function refreshAccessToken() {
    if (!refreshToken.value) {
      clearTokens()
      return false
    }

    try {
      const response = await authApi.refresh(refreshToken.value)
      if (response.success && response.data) {
        setTokens(response.data)
        return true
      }
    } catch {
      clearTokens()
    }
    return false
  }

  async function fetchCurrentUser() {
    try {
      const response = await authApi.me()
      if (response.success && response.data) {
        user.value = response.data
      }
    } catch {
      clearTokens()
    }
  }

  function setTokens(data: LoginResponse) {
    token.value = data.accessToken
    refreshToken.value = data.refreshToken
    localStorage.setItem('token', data.accessToken)
    localStorage.setItem('refreshToken', data.refreshToken)
  }

  function clearTokens() {
    token.value = ''
    refreshToken.value = ''
    user.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('refreshToken')
  }

  return {
    token,
    refreshToken,
    user,
    isLoggedIn,
    isAdmin,
    isGM,
    login,
    logout,
    refreshAccessToken,
    fetchCurrentUser,
    clearTokens
  }
})
