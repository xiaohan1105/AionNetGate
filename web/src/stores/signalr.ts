import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import * as signalR from '@microsoft/signalr'
import { useAuthStore } from './auth'

export const useSignalRStore = defineStore('signalr', () => {
  // 状态
  const dashboardConnection = ref<signalR.HubConnection | null>(null)
  const isConnected = ref(false)
  const isConnecting = ref(false)
  const lastError = ref<string | null>(null)
  const reconnectAttempts = ref(0)

  // 实时数据
  const realTimeStats = ref<RealTimeStats | null>(null)
  const alerts = ref<Alert[]>([])
  const recentActivities = ref<Activity[]>([])

  // 计算属性
  const connectionState = computed(() => {
    if (isConnecting.value) return 'connecting'
    if (isConnected.value) return 'connected'
    return 'disconnected'
  })

  /**
   * 连接 Dashboard Hub
   */
  async function connectDashboard(): Promise<boolean> {
    if (isConnected.value || isConnecting.value) {
      return isConnected.value
    }

    isConnecting.value = true
    lastError.value = null

    try {
      const authStore = useAuthStore()
      if (!authStore.token) {
        throw new Error('未登录，无法连接 SignalR')
      }

      dashboardConnection.value = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/dashboard', {
          accessTokenFactory: () => authStore.token || ''
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            reconnectAttempts.value = retryContext.previousRetryCount + 1
            // 重试策略: 0s, 2s, 5s, 10s, 30s, 然后每 60s
            const delays = [0, 2000, 5000, 10000, 30000]
            return delays[Math.min(retryContext.previousRetryCount, delays.length - 1)] || 60000
          }
        })
        .configureLogging(signalR.LogLevel.Warning)
        .build()

      // 注册事件处理器
      setupEventHandlers()

      await dashboardConnection.value.start()
      isConnected.value = true
      reconnectAttempts.value = 0

      // 订阅统计数据
      await dashboardConnection.value.invoke('SubscribeStats')

      console.info('[SignalR Store] Dashboard Hub 连接成功')
      return true
    } catch (err: any) {
      lastError.value = err.message || '连接失败'
      console.error('[SignalR Store] 连接失败:', err)
      return false
    } finally {
      isConnecting.value = false
    }
  }

  /**
   * 设置事件处理器
   */
  function setupEventHandlers() {
    if (!dashboardConnection.value) return

    // 连接状态变化
    dashboardConnection.value.onclose((error) => {
      isConnected.value = false
      if (error) {
        lastError.value = error.message
        console.error('[SignalR Store] 连接关闭:', error)
      }
    })

    dashboardConnection.value.onreconnecting((error) => {
      isConnected.value = false
      isConnecting.value = true
      console.warn('[SignalR Store] 正在重连...')
    })

    dashboardConnection.value.onreconnected((connectionId) => {
      isConnected.value = true
      isConnecting.value = false
      reconnectAttempts.value = 0
      console.info('[SignalR Store] 重连成功')
      // 重新订阅
      dashboardConnection.value?.invoke('SubscribeStats')
    })

    // 实时统计更新
    dashboardConnection.value.on('StatsUpdated', (stats: RealTimeStats) => {
      realTimeStats.value = stats
    })

    // 玩家上线
    dashboardConnection.value.on('PlayerOnline', (username: string) => {
      addActivity({
        type: 'login',
        message: `${username} 上线`,
        time: new Date().toISOString()
      })
    })

    // 玩家下线
    dashboardConnection.value.on('PlayerOffline', (username: string) => {
      addActivity({
        type: 'logout',
        message: `${username} 下线`,
        time: new Date().toISOString()
      })
    })

    // 告警
    dashboardConnection.value.on('Alert', (type: string, message: string) => {
      addAlert({
        type,
        message,
        time: new Date().toISOString()
      })
    })
  }

  /**
   * 断开连接
   */
  async function disconnect(): Promise<void> {
    if (dashboardConnection.value) {
      try {
        await dashboardConnection.value.stop()
      } catch (err) {
        console.error('[SignalR Store] 断开失败:', err)
      }
      dashboardConnection.value = null
      isConnected.value = false
    }
  }

  /**
   * 添加活动记录
   */
  function addActivity(activity: Activity) {
    recentActivities.value.unshift(activity)
    // 保留最近 50 条
    if (recentActivities.value.length > 50) {
      recentActivities.value.pop()
    }
  }

  /**
   * 添加告警
   */
  function addAlert(alert: Alert) {
    alerts.value.unshift(alert)
    // 保留最近 20 条
    if (alerts.value.length > 20) {
      alerts.value.pop()
    }
  }

  /**
   * 清除告警
   */
  function clearAlerts() {
    alerts.value = []
  }

  return {
    // 状态
    dashboardConnection,
    isConnected,
    isConnecting,
    lastError,
    reconnectAttempts,
    connectionState,
    // 实时数据
    realTimeStats,
    alerts,
    recentActivities,
    // 方法
    connectDashboard,
    disconnect,
    addActivity,
    addAlert,
    clearAlerts
  }
})

// 类型定义
export interface RealTimeStats {
  totalAccounts: number
  todayNewAccounts: number
  onlineCount: number
  blockedIpCount: number
  serverStatus: string
  uptime: string
  // 实时监控指标
  cpuUsage?: number
  memoryUsageMB?: number
  currentConnections?: number
  packetsReceived?: number
  packetsSent?: number
  timestamp?: string
}

export interface Activity {
  type: 'login' | 'logout' | 'register' | 'order' | 'ban' | 'cheat' | string
  message: string
  ip?: string
  time: string
}

export interface Alert {
  type: string
  message: string
  time: string
  severity?: 'info' | 'warning' | 'error'
}
