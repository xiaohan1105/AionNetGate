import { ref, onUnmounted } from 'vue'
import * as signalR from '@microsoft/signalr'
import { useAuthStore } from '@/stores/auth'

export interface SignalROptions {
  /** 自动重连 */
  autoReconnect?: boolean
  /** 日志级别 */
  logLevel?: signalR.LogLevel
}

/**
 * SignalR 连接管理 Composable
 */
export function useSignalR(hubUrl: string, options: SignalROptions = {}) {
  const {
    autoReconnect = true,
    logLevel = signalR.LogLevel.Warning
  } = options

  const connection = ref<signalR.HubConnection | null>(null)
  const connected = ref(false)
  const connecting = ref(false)
  const error = ref<Error | null>(null)

  /**
   * 建立连接
   */
  async function connect(): Promise<boolean> {
    if (connected.value || connecting.value) {
      return connected.value
    }

    connecting.value = true
    error.value = null

    try {
      const authStore = useAuthStore()

      const builder = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl, {
          accessTokenFactory: () => authStore.token || ''
        })
        .configureLogging(logLevel)

      if (autoReconnect) {
        // 自动重连策略: 0s, 2s, 5s, 10s, 30s, 然后每 30s 重试
        builder.withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      }

      connection.value = builder.build()

      // 监听连接状态变化
      connection.value.onclose((err) => {
        connected.value = false
        if (err) {
          error.value = err
          console.error('[SignalR] 连接关闭:', err)
        }
      })

      connection.value.onreconnecting((err) => {
        connected.value = false
        console.warn('[SignalR] 正在重连...', err)
      })

      connection.value.onreconnected((connectionId) => {
        connected.value = true
        console.info('[SignalR] 重连成功:', connectionId)
      })

      await connection.value.start()
      connected.value = true
      console.info('[SignalR] 连接成功:', hubUrl)
      return true
    } catch (err) {
      error.value = err as Error
      console.error('[SignalR] 连接失败:', err)
      return false
    } finally {
      connecting.value = false
    }
  }

  /**
   * 断开连接
   */
  async function disconnect(): Promise<void> {
    if (connection.value) {
      try {
        await connection.value.stop()
      } catch (err) {
        console.error('[SignalR] 断开连接失败:', err)
      }
      connection.value = null
      connected.value = false
    }
  }

  /**
   * 订阅事件
   */
  function on<T = any>(event: string, callback: (data: T) => void): void {
    if (connection.value) {
      connection.value.on(event, callback)
    }
  }

  /**
   * 取消订阅事件
   */
  function off(event: string, callback?: (...args: any[]) => void): void {
    if (connection.value) {
      if (callback) {
        connection.value.off(event, callback)
      } else {
        connection.value.off(event)
      }
    }
  }

  /**
   * 调用服务器方法
   */
  async function invoke<T = any>(method: string, ...args: any[]): Promise<T | undefined> {
    if (!connection.value || !connected.value) {
      console.warn('[SignalR] 未连接，无法调用:', method)
      return undefined
    }

    try {
      return await connection.value.invoke<T>(method, ...args)
    } catch (err) {
      console.error('[SignalR] 调用失败:', method, err)
      throw err
    }
  }

  /**
   * 发送消息（无返回值）
   */
  async function send(method: string, ...args: any[]): Promise<void> {
    if (!connection.value || !connected.value) {
      console.warn('[SignalR] 未连接，无法发送:', method)
      return
    }

    try {
      await connection.value.send(method, ...args)
    } catch (err) {
      console.error('[SignalR] 发送失败:', method, err)
      throw err
    }
  }

  // 组件卸载时自动断开连接
  onUnmounted(() => {
    disconnect()
  })

  return {
    connection,
    connected,
    connecting,
    error,
    connect,
    disconnect,
    on,
    off,
    invoke,
    send
  }
}

/**
 * Dashboard Hub 专用 Composable
 */
export function useDashboardHub() {
  const signalr = useSignalR('/hubs/dashboard')

  /**
   * 订阅统计数据更新
   */
  async function subscribeStats(): Promise<void> {
    await signalr.invoke('SubscribeStats')
  }

  /**
   * 取消订阅统计数据
   */
  async function unsubscribeStats(): Promise<void> {
    await signalr.invoke('UnsubscribeStats')
  }

  /**
   * 监听统计数据更新
   */
  function onStatsUpdated(callback: (stats: DashboardStats) => void): void {
    signalr.on('StatsUpdated', callback)
  }

  /**
   * 监听玩家上线
   */
  function onPlayerOnline(callback: (username: string) => void): void {
    signalr.on('PlayerOnline', callback)
  }

  /**
   * 监听玩家下线
   */
  function onPlayerOffline(callback: (username: string) => void): void {
    signalr.on('PlayerOffline', callback)
  }

  /**
   * 监听告警
   */
  function onAlert(callback: (type: string, message: string) => void): void {
    signalr.on('Alert', callback)
  }

  return {
    ...signalr,
    subscribeStats,
    unsubscribeStats,
    onStatsUpdated,
    onPlayerOnline,
    onPlayerOffline,
    onAlert
  }
}

// 类型定义
export interface DashboardStats {
  totalAccounts: number
  todayNewAccounts: number
  onlineCount: number
  blockedIpCount: number
  serverStatus: string
  uptime: string
  cpuUsage?: number
  memoryUsage?: number
  packetsReceived?: number
  packetsSent?: number
}
