<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import {
  NCard, NTabs, NTabPane, NForm, NFormItem, NInput, NInputNumber, NSwitch,
  NSelect, NButton, NSpace, NSpin, NAlert, NDescriptions, NDescriptionsItem,
  NTag, NIcon, NList, NListItem, NScrollbar, useMessage
} from 'naive-ui'
import {
  ServerOutline, ShieldOutline, ServerOutline as DatabaseIcon,
  GameControllerOutline, FlameOutline, RocketOutline, BugOutline,
  MailOutline, DocumentTextOutline, InformationCircleOutline,
  SaveOutline, RefreshOutline
} from '@vicons/ionicons5'
import { settingsApi } from '@/api/settings'
import type {
  AllSettings, ConfigCategory, SystemInfo, LogEntry,
  ServerSettings, SecuritySettings, DatabaseSettings, GameDatabaseSettings,
  GatewaySettings, FirewallSettings, LauncherSettings, CheatDetectionSettings,
  EmailSettings, LoggingSettings
} from '@/api/settings'

const message = useMessage()

// 状态
const loading = ref(true)
const saving = ref(false)
const activeTab = ref('server')
const categories = ref<ConfigCategory[]>([])
const settings = ref<AllSettings | null>(null)
const systemInfo = ref<SystemInfo | null>(null)
const logs = ref<LogEntry[]>([])
const logsLoading = ref(false)
const hasChanges = ref(false)

// 本地编辑副本
const localSettings = ref<AllSettings | null>(null)

// Tab 图标映射
const tabIcons: Record<string, any> = {
  server: ServerOutline,
  security: ShieldOutline,
  database: DatabaseIcon,
  gameDatabase: GameControllerOutline,
  gateway: FlameOutline,
  firewall: ShieldOutline,
  launcher: RocketOutline,
  cheatDetection: BugOutline,
  email: MailOutline,
  logging: DocumentTextOutline
}

// 日志级别选项
const logLevelOptions = [
  { label: 'Verbose', value: 'Verbose' },
  { label: 'Debug', value: 'Debug' },
  { label: 'Information', value: 'Information' },
  { label: 'Warning', value: 'Warning' },
  { label: 'Error', value: 'Error' },
  { label: 'Fatal', value: 'Fatal' }
]

// 数据库提供商选项
const dbProviderOptions = [
  { label: 'SQLite', value: 'SQLite' },
  { label: 'MySQL', value: 'MySQL' },
  { label: 'MSSQL', value: 'MSSQL' }
]

// 加载数据
async function loadData() {
  loading.value = true
  try {
    const [categoriesRes, settingsRes, infoRes] = await Promise.all([
      settingsApi.getCategories(),
      settingsApi.getAll(),
      settingsApi.getSystemInfo()
    ])

    if (categoriesRes.success) categories.value = categoriesRes.data!
    if (settingsRes.success) {
      settings.value = settingsRes.data!
      localSettings.value = JSON.parse(JSON.stringify(settingsRes.data))
    }
    if (infoRes.success) systemInfo.value = infoRes.data!
  } catch (err) {
    message.error('加载配置失败')
  } finally {
    loading.value = false
  }
}

// 加载日志
async function loadLogs() {
  logsLoading.value = true
  try {
    const res = await settingsApi.getLogs(100)
    if (res.success) logs.value = res.data!
  } finally {
    logsLoading.value = false
  }
}

// 保存配置
async function saveSettings(category: string) {
  if (!localSettings.value) return

  saving.value = true
  try {
    const data = (localSettings.value as any)[category]
    const res = await settingsApi.updateCategory(category, data)
    if (res.success) {
      message.success(res.message || '保存成功')
      hasChanges.value = false
      // 重新加载配置
      await loadData()
    } else {
      message.error(res.message || '保存失败')
    }
  } catch (err) {
    message.error('保存失败')
  } finally {
    saving.value = false
  }
}

// 重置配置
function resetSettings(category: string) {
  if (settings.value && localSettings.value) {
    (localSettings.value as any)[category] = JSON.parse(
      JSON.stringify((settings.value as any)[category])
    )
    hasChanges.value = false
    message.info('已重置为原始值')
  }
}

// 标记有修改
function markChanged() {
  hasChanges.value = true
}

// 格式化运行时间
function formatUptime(uptime: string): string {
  if (!uptime) return '-'
  // 格式: "00:05:30.1234567" -> "5分30秒"
  const match = uptime.match(/(\d+):(\d+):(\d+)/)
  if (match) {
    const hours = parseInt(match[1])
    const minutes = parseInt(match[2])
    const seconds = parseInt(match[3])
    if (hours > 0) return `${hours}小时${minutes}分`
    if (minutes > 0) return `${minutes}分${seconds}秒`
    return `${seconds}秒`
  }
  return uptime
}

// 日志级别颜色
function getLogLevelType(level: string): 'default' | 'info' | 'success' | 'warning' | 'error' {
  switch (level.toLowerCase()) {
    case 'error':
    case 'fatal':
      return 'error'
    case 'warning':
      return 'warning'
    case 'info':
      return 'info'
    case 'debug':
      return 'default'
    default:
      return 'default'
  }
}

onMounted(() => {
  loadData()
})
</script>

<template>
  <n-spin :show="loading">
    <n-card title="系统设置">
      <n-tabs v-model:value="activeTab" type="line" animated>
        <!-- 服务器配置 -->
        <n-tab-pane name="server" tab="服务器">
          <template #tab>
            <n-space align="center" :size="4">
              <n-icon :component="tabIcons.server" />
              <span>服务器</span>
            </n-space>
          </template>
          <n-form v-if="localSettings" label-placement="left" label-width="140" @input="markChanged">
            <n-form-item label="绑定地址">
              <n-input v-model:value="localSettings.server.bindAddress" placeholder="0.0.0.0" />
            </n-form-item>
            <n-form-item label="监听端口">
              <n-input-number v-model:value="localSettings.server.port" :min="1" :max="65535" />
            </n-form-item>
            <n-form-item label="最大连接数">
              <n-input-number v-model:value="localSettings.server.maxConnections" :min="100" :max="100000" />
            </n-form-item>
            <n-form-item label="连接超时(秒)">
              <n-input-number v-model:value="localSettings.server.connectionTimeout" :min="30" :max="3600" />
            </n-form-item>
            <n-form-item label="心跳间隔(秒)">
              <n-input-number v-model:value="localSettings.server.heartbeatInterval" :min="5" :max="300" />
            </n-form-item>
            <n-form-item label="接收缓冲区(字节)">
              <n-input-number v-model:value="localSettings.server.receiveBufferSize" :min="1024" :max="65536" />
            </n-form-item>
            <n-form-item label="发送缓冲区(字节)">
              <n-input-number v-model:value="localSettings.server.sendBufferSize" :min="1024" :max="65536" />
            </n-form-item>
          </n-form>
          <n-space justify="end" style="margin-top: 16px">
            <n-button @click="resetSettings('server')">重置</n-button>
            <n-button type="primary" :loading="saving" @click="saveSettings('server')">
              <template #icon><n-icon :component="SaveOutline" /></template>
              保存
            </n-button>
          </n-space>
        </n-tab-pane>

        <!-- 安全配置 -->
        <n-tab-pane name="security" tab="安全">
          <template #tab>
            <n-space align="center" :size="4">
              <n-icon :component="tabIcons.security" />
              <span>安全</span>
            </n-space>
          </template>
          <n-form v-if="localSettings" label-placement="left" label-width="160" @input="markChanged">
            <n-form-item label="最大登录尝试">
              <n-input-number v-model:value="localSettings.security.maxLoginAttempts" :min="1" :max="100" />
            </n-form-item>
            <n-form-item label="账户锁定时长(分钟)">
              <n-input-number v-model:value="localSettings.security.accountLockoutMinutes" :min="1" :max="1440" />
            </n-form-item>
            <n-form-item label="Token过期时间(分钟)">
              <n-input-number v-model:value="localSettings.security.accessTokenExpirationMinutes" :min="5" :max="1440" />
            </n-form-item>
            <n-form-item label="刷新Token过期(天)">
              <n-input-number v-model:value="localSettings.security.refreshTokenExpirationDays" :min="1" :max="365" />
            </n-form-item>
            <n-form-item label="启用IP白名单">
              <n-switch v-model:value="localSettings.security.enableIpWhitelist" />
            </n-form-item>
            <n-form-item label="加密密钥">
              <n-input v-model:value="localSettings.security.encryptionKey" type="password" show-password-on="click" placeholder="********" />
            </n-form-item>
            <n-form-item label="JWT密钥">
              <n-input v-model:value="localSettings.security.jwtSecretKey" type="password" show-password-on="click" placeholder="********" />
            </n-form-item>
          </n-form>
          <n-space justify="end" style="margin-top: 16px">
            <n-button @click="resetSettings('security')">重置</n-button>
            <n-button type="primary" :loading="saving" @click="saveSettings('security')">
              <template #icon><n-icon :component="SaveOutline" /></template>
              保存
            </n-button>
          </n-space>
        </n-tab-pane>

        <!-- 数据库配置 -->
        <n-tab-pane name="database" tab="数据库">
          <template #tab>
            <n-space align="center" :size="4">
              <n-icon :component="tabIcons.database" />
              <span>数据库</span>
            </n-space>
          </template>
          <n-form v-if="localSettings" label-placement="left" label-width="140" @input="markChanged">
            <n-form-item label="数据库类型">
              <n-select v-model:value="localSettings.database.provider" :options="dbProviderOptions" />
            </n-form-item>
            <n-form-item label="连接字符串">
              <n-input v-model:value="localSettings.database.connectionString" type="password" show-password-on="click" placeholder="********" />
            </n-form-item>
            <n-form-item label="命令超时(秒)">
              <n-input-number v-model:value="localSettings.database.commandTimeout" :min="5" :max="300" />
            </n-form-item>
            <n-form-item label="敏感数据日志">
              <n-switch v-model:value="localSettings.database.enableSensitiveDataLogging" />
            </n-form-item>
            <n-form-item label="最大重试次数">
              <n-input-number v-model:value="localSettings.database.maxRetryCount" :min="0" :max="10" />
            </n-form-item>
          </n-form>
          <n-space justify="end" style="margin-top: 16px">
            <n-button @click="resetSettings('database')">重置</n-button>
            <n-button type="primary" :loading="saving" @click="saveSettings('database')">
              <template #icon><n-icon :component="SaveOutline" /></template>
              保存
            </n-button>
          </n-space>
        </n-tab-pane>

        <!-- 游戏数据库配置 -->
        <n-tab-pane name="gameDatabase" tab="游戏数据库">
          <template #tab>
            <n-space align="center" :size="4">
              <n-icon :component="tabIcons.gameDatabase" />
              <span>游戏库</span>
            </n-space>
          </template>
          <n-form v-if="localSettings" label-placement="left" label-width="140" @input="markChanged">
            <n-form-item label="启用">
              <n-switch v-model:value="localSettings.gameDatabase.enabled" />
            </n-form-item>
            <n-form-item label="数据库类型">
              <n-select v-model:value="localSettings.gameDatabase.provider" :options="dbProviderOptions" />
            </n-form-item>
            <n-form-item label="主机地址">
              <n-input v-model:value="localSettings.gameDatabase.host" placeholder="localhost" />
            </n-form-item>
            <n-form-item label="端口">
              <n-input-number v-model:value="localSettings.gameDatabase.port" :min="1" :max="65535" />
            </n-form-item>
            <n-form-item label="数据库名">
              <n-input v-model:value="localSettings.gameDatabase.databaseName" placeholder="aion_gs" />
            </n-form-item>
            <n-form-item label="用户名">
              <n-input v-model:value="localSettings.gameDatabase.username" />
            </n-form-item>
            <n-form-item label="密码">
              <n-input v-model:value="localSettings.gameDatabase.password" type="password" show-password-on="click" placeholder="********" />
            </n-form-item>
          </n-form>
          <n-space justify="end" style="margin-top: 16px">
            <n-button @click="resetSettings('gameDatabase')">重置</n-button>
            <n-button type="primary" :loading="saving" @click="saveSettings('gameDatabase')">
              <template #icon><n-icon :component="SaveOutline" /></template>
              保存
            </n-button>
          </n-space>
        </n-tab-pane>

        <!-- 网关高级配置 -->
        <n-tab-pane name="gateway" tab="网关">
          <template #tab>
            <n-space align="center" :size="4">
              <n-icon :component="tabIcons.gateway" />
              <span>网关</span>
            </n-space>
          </template>
          <n-form v-if="localSettings" label-placement="left" label-width="160" @input="markChanged">
            <n-form-item label="启用数据包压缩">
              <n-switch v-model:value="localSettings.gateway.enablePacketCompression" />
            </n-form-item>
            <n-form-item label="压缩阈值(字节)">
              <n-input-number v-model:value="localSettings.gateway.compressionThreshold" :min="128" :max="65536" />
            </n-form-item>
            <n-form-item label="最大包大小(字节)">
              <n-input-number v-model:value="localSettings.gateway.maxPacketSize" :min="1024" :max="1048576" />
            </n-form-item>
            <n-form-item label="启用数据包日志">
              <n-switch v-model:value="localSettings.gateway.enablePacketLogging" />
            </n-form-item>
            <n-form-item label="数据包队列大小">
              <n-input-number v-model:value="localSettings.gateway.packetQueueSize" :min="100" :max="100000" />
            </n-form-item>
          </n-form>
          <n-space justify="end" style="margin-top: 16px">
            <n-button @click="resetSettings('gateway')">重置</n-button>
            <n-button type="primary" :loading="saving" @click="saveSettings('gateway')">
              <template #icon><n-icon :component="SaveOutline" /></template>
              保存
            </n-button>
          </n-space>
        </n-tab-pane>

        <!-- 防火墙配置 -->
        <n-tab-pane name="firewall" tab="防火墙">
          <template #tab>
            <n-space align="center" :size="4">
              <n-icon :component="tabIcons.firewall" />
              <span>防火墙</span>
            </n-space>
          </template>
          <n-form v-if="localSettings" label-placement="left" label-width="180" @input="markChanged">
            <n-form-item label="启用防火墙集成">
              <n-switch v-model:value="localSettings.firewall.enabled" />
            </n-form-item>
            <n-form-item label="自动添加到白名单">
              <n-switch v-model:value="localSettings.firewall.autoAddToWhitelist" />
            </n-form-item>
            <n-form-item label="自动封禁攻击者">
              <n-switch v-model:value="localSettings.firewall.autoBlockAttackers" />
            </n-form-item>
            <n-form-item label="受保护端口">
              <n-input v-model:value="localSettings.firewall.protectedPorts" placeholder="7777,10241,2106" />
            </n-form-item>
            <n-form-item label="白名单过期时间(小时)">
              <n-input-number v-model:value="localSettings.firewall.whitelistExpirationHours" :min="0" :max="720" />
            </n-form-item>
            <n-form-item label="黑名单过期时间(小时)">
              <n-input-number v-model:value="localSettings.firewall.blacklistExpirationHours" :min="0" :max="8760" />
            </n-form-item>
            <n-form-item label="每秒最大连接数">
              <n-input-number v-model:value="localSettings.firewall.maxConnectionsPerSecond" :min="1" :max="10000" />
            </n-form-item>
          </n-form>
          <n-space justify="end" style="margin-top: 16px">
            <n-button @click="resetSettings('firewall')">重置</n-button>
            <n-button type="primary" :loading="saving" @click="saveSettings('firewall')">
              <template #icon><n-icon :component="SaveOutline" /></template>
              保存
            </n-button>
          </n-space>
        </n-tab-pane>

        <!-- 启动器配置 -->
        <n-tab-pane name="launcher" tab="启动器">
          <template #tab>
            <n-space align="center" :size="4">
              <n-icon :component="tabIcons.launcher" />
              <span>启动器</span>
            </n-space>
          </template>
          <n-form v-if="localSettings" label-placement="left" label-width="160" @input="markChanged">
            <n-form-item label="启用自动更新">
              <n-switch v-model:value="localSettings.launcher.autoUpdateEnabled" />
            </n-form-item>
            <n-form-item label="更新服务器URL">
              <n-input v-model:value="localSettings.launcher.updateServerUrl" placeholder="https://update.example.com" />
            </n-form-item>
            <n-form-item label="启用转发器">
              <n-switch v-model:value="localSettings.launcher.forwarderEnabled" />
            </n-form-item>
            <n-form-item label="转发器密码">
              <n-input v-model:value="localSettings.launcher.forwarderPassword" type="password" show-password-on="click" placeholder="********" />
            </n-form-item>
            <n-form-item label="允许多开">
              <n-switch v-model:value="localSettings.launcher.allowMultipleInstances" />
            </n-form-item>
          </n-form>
          <n-space justify="end" style="margin-top: 16px">
            <n-button @click="resetSettings('launcher')">重置</n-button>
            <n-button type="primary" :loading="saving" @click="saveSettings('launcher')">
              <template #icon><n-icon :component="SaveOutline" /></template>
              保存
            </n-button>
          </n-space>
        </n-tab-pane>

        <!-- 外挂检测配置 -->
        <n-tab-pane name="cheatDetection" tab="反作弊">
          <template #tab>
            <n-space align="center" :size="4">
              <n-icon :component="tabIcons.cheatDetection" />
              <span>反作弊</span>
            </n-space>
          </template>
          <n-form v-if="localSettings" label-placement="left" label-width="160" @input="markChanged">
            <n-form-item label="启用外挂检测">
              <n-switch v-model:value="localSettings.cheatDetection.enabled" />
            </n-form-item>
            <n-form-item label="检测间隔(秒)">
              <n-input-number v-model:value="localSettings.cheatDetection.checkInterval" :min="5" :max="300" />
            </n-form-item>
            <n-form-item label="进程扫描">
              <n-switch v-model:value="localSettings.cheatDetection.enableProcessScan" />
            </n-form-item>
            <n-form-item label="内存扫描">
              <n-switch v-model:value="localSettings.cheatDetection.enableMemoryScan" />
            </n-form-item>
            <n-form-item label="文件扫描">
              <n-switch v-model:value="localSettings.cheatDetection.enableFileScan" />
            </n-form-item>
            <n-form-item label="检测后自动封禁">
              <n-switch v-model:value="localSettings.cheatDetection.autoBanOnDetection" />
            </n-form-item>
            <n-form-item label="封禁时长(小时)">
              <n-input-number v-model:value="localSettings.cheatDetection.banDurationHours" :min="1" :max="8760" />
            </n-form-item>
          </n-form>
          <n-space justify="end" style="margin-top: 16px">
            <n-button @click="resetSettings('cheatDetection')">重置</n-button>
            <n-button type="primary" :loading="saving" @click="saveSettings('cheatDetection')">
              <template #icon><n-icon :component="SaveOutline" /></template>
              保存
            </n-button>
          </n-space>
        </n-tab-pane>

        <!-- 邮件配置 -->
        <n-tab-pane name="email" tab="邮件">
          <template #tab>
            <n-space align="center" :size="4">
              <n-icon :component="tabIcons.email" />
              <span>邮件</span>
            </n-space>
          </template>
          <n-form v-if="localSettings" label-placement="left" label-width="140" @input="markChanged">
            <n-form-item label="启用邮件通知">
              <n-switch v-model:value="localSettings.email.enabled" />
            </n-form-item>
            <n-form-item label="SMTP服务器">
              <n-input v-model:value="localSettings.email.smtpHost" placeholder="smtp.example.com" />
            </n-form-item>
            <n-form-item label="SMTP端口">
              <n-input-number v-model:value="localSettings.email.smtpPort" :min="1" :max="65535" />
            </n-form-item>
            <n-form-item label="使用SSL">
              <n-switch v-model:value="localSettings.email.useSsl" />
            </n-form-item>
            <n-form-item label="用户名">
              <n-input v-model:value="localSettings.email.username" />
            </n-form-item>
            <n-form-item label="密码">
              <n-input v-model:value="localSettings.email.password" type="password" show-password-on="click" placeholder="********" />
            </n-form-item>
            <n-form-item label="发件人地址">
              <n-input v-model:value="localSettings.email.fromAddress" placeholder="noreply@example.com" />
            </n-form-item>
            <n-form-item label="发件人名称">
              <n-input v-model:value="localSettings.email.fromName" placeholder="AionNetGate" />
            </n-form-item>
          </n-form>
          <n-space justify="end" style="margin-top: 16px">
            <n-button @click="resetSettings('email')">重置</n-button>
            <n-button type="primary" :loading="saving" @click="saveSettings('email')">
              <template #icon><n-icon :component="SaveOutline" /></template>
              保存
            </n-button>
          </n-space>
        </n-tab-pane>

        <!-- 日志配置 -->
        <n-tab-pane name="logging" tab="日志">
          <template #tab>
            <n-space align="center" :size="4">
              <n-icon :component="tabIcons.logging" />
              <span>日志</span>
            </n-space>
          </template>
          <n-form v-if="localSettings" label-placement="left" label-width="140" @input="markChanged">
            <n-form-item label="最低日志级别">
              <n-select v-model:value="localSettings.logging.minimumLevel" :options="logLevelOptions" />
            </n-form-item>
            <n-form-item label="控制台输出">
              <n-switch v-model:value="localSettings.logging.enableConsole" />
            </n-form-item>
            <n-form-item label="文件输出">
              <n-switch v-model:value="localSettings.logging.enableFile" />
            </n-form-item>
            <n-form-item label="日志目录">
              <n-input v-model:value="localSettings.logging.logFilePath" placeholder="logs" />
            </n-form-item>
            <n-form-item label="保留天数">
              <n-input-number v-model:value="localSettings.logging.retentionDays" :min="1" :max="365" />
            </n-form-item>
            <n-form-item label="最大文件大小">
              <n-input-number v-model:value="localSettings.logging.maxFileSize" :min="1048576" :max="104857600" />
              <span style="margin-left: 8px; color: #999">字节</span>
            </n-form-item>
          </n-form>
          <n-space justify="end" style="margin-top: 16px">
            <n-button @click="resetSettings('logging')">重置</n-button>
            <n-button type="primary" :loading="saving" @click="saveSettings('logging')">
              <template #icon><n-icon :component="SaveOutline" /></template>
              保存
            </n-button>
          </n-space>
        </n-tab-pane>

        <!-- 系统信息 -->
        <n-tab-pane name="info" tab="系统信息">
          <template #tab>
            <n-space align="center" :size="4">
              <n-icon :component="InformationCircleOutline" />
              <span>系统</span>
            </n-space>
          </template>
          <n-descriptions v-if="systemInfo" :column="2" bordered>
            <n-descriptions-item label="版本">{{ systemInfo.version }}</n-descriptions-item>
            <n-descriptions-item label="环境">
              <n-tag :type="systemInfo.environment === 'Production' ? 'success' : 'warning'">
                {{ systemInfo.environment }}
              </n-tag>
            </n-descriptions-item>
            <n-descriptions-item label="主机名">{{ systemInfo.machineName }}</n-descriptions-item>
            <n-descriptions-item label="操作系统">{{ systemInfo.osVersion }}</n-descriptions-item>
            <n-descriptions-item label="CPU核心数">{{ systemInfo.processorCount }}</n-descriptions-item>
            <n-descriptions-item label="内存使用">{{ systemInfo.workingSet }} MB</n-descriptions-item>
            <n-descriptions-item label="启动时间">
              {{ new Date(systemInfo.startTime).toLocaleString('zh-CN') }}
            </n-descriptions-item>
            <n-descriptions-item label="运行时间">{{ formatUptime(systemInfo.uptime) }}</n-descriptions-item>
          </n-descriptions>

          <n-card title="运行日志" style="margin-top: 16px" size="small">
            <template #header-extra>
              <n-button size="small" @click="loadLogs" :loading="logsLoading">
                <template #icon><n-icon :component="RefreshOutline" /></template>
                刷新
              </n-button>
            </template>
            <n-scrollbar style="max-height: 400px">
              <n-list v-if="logs.length" :hoverable="true" size="small">
                <n-list-item v-for="(log, index) in logs" :key="index">
                  <n-space align="center" :size="8">
                    <span style="color: #999; font-size: 12px; width: 60px">{{ log.time }}</span>
                    <n-tag :type="getLogLevelType(log.level)" size="small" style="width: 60px">
                      {{ log.level }}
                    </n-tag>
                    <span style="font-size: 13px">{{ log.message }}</span>
                  </n-space>
                </n-list-item>
              </n-list>
              <div v-else style="text-align: center; padding: 20px; color: #999">
                点击刷新按钮加载日志
              </div>
            </n-scrollbar>
          </n-card>
        </n-tab-pane>
      </n-tabs>

      <n-alert v-if="hasChanges" type="warning" style="margin-top: 16px">
        配置已修改，请记得保存。修改将在服务重启后生效。
      </n-alert>
    </n-card>
  </n-spin>
</template>

<style scoped>
:deep(.n-tabs-nav) {
  flex-wrap: wrap;
}
</style>
