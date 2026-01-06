// API 响应类型
export interface ApiResponse<T = any> {
  success: boolean
  message: string
  data?: T
  errorCode?: string
  timestamp: number
}

export interface PagedResponse<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasPrevious: boolean
  hasNext: boolean
}

// 用户相关
export interface UserInfo {
  id: number
  username: string
  email?: string
  role: number
  roleName: string
}

export interface LoginResponse {
  accessToken: string
  refreshToken: string
  expiresIn: number
  user: UserInfo
}

// 仪表盘
export interface DashboardStats {
  totalAccounts: number
  todayNewAccounts: number
  onlineCount: number
  blockedIpCount: number
  serverStatus: string
  uptime: string
}

export interface OnlineTrendPoint {
  time: string
  count: number
}

export interface ActivityLog {
  id: number
  type: string
  message: string
  ip?: string
  time: string
}

// 玩家
export interface Player {
  id: number
  username: string
  email?: string
  status: number
  statusText: string
  role: number
  roleText: string
  lastLoginAt?: string
  lastLoginIp?: string
  createdAt: string
}

export interface PlayerDetail extends Player {
  loginAttempts: number
  lockedUntil?: string
  updatedAt: string
  activeSessions: number
  hardwareFingerprints: HardwareFingerprint[]
}

export interface HardwareFingerprint {
  id: number
  fingerprintHash: string
  cpuId?: string
  diskId?: string
  macAddress?: string
  firstSeenAt: string
  lastSeenAt: string
}
