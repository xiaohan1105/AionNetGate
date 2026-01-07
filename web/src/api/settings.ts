import http from './http'
import type { ApiResponse } from './types'

// 配置分类
export interface ConfigCategory {
  id: string
  name: string
  description: string
}

// 所有配置
export interface AllSettings {
  server: ServerSettings
  security: SecuritySettings
  database: DatabaseSettings
  gameDatabase: GameDatabaseSettings
  gateway: GatewaySettings
  firewall: FirewallSettings
  launcher: LauncherSettings
  cheatDetection: CheatDetectionSettings
  email: EmailSettings
  logging: LoggingSettings
}

export interface ServerSettings {
  bindAddress: string
  port: number
  maxConnections: number
  connectionTimeout: number
  heartbeatInterval: number
  receiveBufferSize: number
  sendBufferSize: number
}

export interface SecuritySettings {
  maxLoginAttempts: number
  accountLockoutMinutes: number
  accessTokenExpirationMinutes: number
  refreshTokenExpirationDays: number
  enableIpWhitelist: boolean
  encryptionKey?: string
  jwtSecretKey?: string
}

export interface DatabaseSettings {
  provider: string
  connectionString: string
  commandTimeout: number
  enableSensitiveDataLogging: boolean
  maxRetryCount: number
}

export interface GameDatabaseSettings {
  provider: string
  host: string
  port: number
  databaseName: string
  username: string
  password?: string
  enabled: boolean
}

export interface GatewaySettings {
  enablePacketCompression: boolean
  compressionThreshold: number
  maxPacketSize: number
  enablePacketLogging: boolean
  packetQueueSize: number
}

export interface FirewallSettings {
  enabled: boolean
  autoAddToWhitelist: boolean
  autoBlockAttackers: boolean
  protectedPorts: string
  whitelistExpirationHours: number
  blacklistExpirationHours: number
  maxConnectionsPerSecond: number
}

export interface LauncherSettings {
  autoUpdateEnabled: boolean
  updateServerUrl: string
  forwarderEnabled: boolean
  forwarderPassword?: string
  allowMultipleInstances: boolean
}

export interface CheatDetectionSettings {
  enabled: boolean
  checkInterval: number
  enableProcessScan: boolean
  enableMemoryScan: boolean
  enableFileScan: boolean
  autoBanOnDetection: boolean
  banDurationHours: number
}

export interface EmailSettings {
  enabled: boolean
  smtpHost: string
  smtpPort: number
  useSsl: boolean
  username: string
  password?: string
  fromAddress: string
  fromName: string
}

export interface LoggingSettings {
  minimumLevel: string
  enableConsole: boolean
  enableFile: boolean
  logFilePath: string
  retentionDays: number
  maxFileSize: number
}

export interface SystemInfo {
  version: string
  environment: string
  machineName: string
  osVersion: string
  processorCount: number
  workingSet: number
  startTime: string
  uptime: string
}

export interface LogEntry {
  time: string
  level: string
  message: string
}

export const settingsApi = {
  /**
   * 获取配置分类列表
   */
  getCategories(): Promise<ApiResponse<ConfigCategory[]>> {
    return http.get('/settings/categories')
  },

  /**
   * 获取所有配置
   */
  getAll(): Promise<ApiResponse<AllSettings>> {
    return http.get('/settings')
  },

  /**
   * 获取指定分类配置
   */
  getCategory<T = any>(category: string): Promise<ApiResponse<T>> {
    return http.get(`/settings/${category}`)
  },

  /**
   * 更新指定分类配置
   */
  updateCategory(category: string, data: Record<string, any>): Promise<ApiResponse<null>> {
    return http.put(`/settings/${category}`, data)
  },

  /**
   * 验证配置
   */
  validate(settings: Partial<AllSettings>): Promise<ApiResponse<null>> {
    return http.post('/settings/validate', settings)
  },

  /**
   * 获取系统信息
   */
  getSystemInfo(): Promise<ApiResponse<SystemInfo>> {
    return http.get('/settings/info')
  },

  /**
   * 获取运行日志
   */
  getLogs(lines: number = 100, level?: string): Promise<ApiResponse<LogEntry[]>> {
    return http.get('/settings/logs', { params: { lines, level } })
  }
}
