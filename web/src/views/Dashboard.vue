<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, computed } from 'vue'
import {
  NGrid, NGridItem, NCard, NStatistic, NSpace, NSpin, NList, NListItem,
  NTag, NEmpty, NBadge, NTooltip, NIcon
} from 'naive-ui'
import { WifiOutline, CloudOfflineOutline } from '@vicons/ionicons5'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { LineChart, BarChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, LegendComponent } from 'echarts/components'
import VChart from 'vue-echarts'
import { dashboardApi } from '@/api/dashboard'
import { useSignalRStore } from '@/stores/signalr'
import type { DashboardStats, OnlineTrendPoint, ActivityLog } from '@/api/types'

// 监控图表组件
import ResourceChart from '@/components/dashboard/ResourceChart.vue'
import ConnectionChart from '@/components/dashboard/ConnectionChart.vue'
import PacketChart from '@/components/dashboard/PacketChart.vue'

use([CanvasRenderer, LineChart, BarChart, GridComponent, TooltipComponent, LegendComponent])

// SignalR Store
const signalrStore = useSignalRStore()

// 本地状态
const loading = ref(true)
const stats = ref<DashboardStats | null>(null)
const trendData = ref<OnlineTrendPoint[]>([])
const activities = ref<ActivityLog[]>([])

// 计算属性 - 合并 API 数据和实时数据
const displayStats = computed(() => {
  const realTime = signalrStore.realTimeStats
  if (realTime && signalrStore.isConnected) {
    return {
      totalAccounts: realTime.totalAccounts ?? stats.value?.totalAccounts ?? 0,
      todayNewAccounts: realTime.todayNewAccounts ?? stats.value?.todayNewAccounts ?? 0,
      onlineCount: realTime.onlineCount ?? stats.value?.onlineCount ?? 0,
      blockedIpCount: realTime.blockedIpCount ?? stats.value?.blockedIpCount ?? 0,
      serverStatus: realTime.serverStatus ?? stats.value?.serverStatus ?? 'unknown',
      uptime: realTime.uptime ?? stats.value?.uptime ?? '-',
      cpuUsage: realTime.cpuUsage ?? 0,
      memoryUsageMB: realTime.memoryUsageMB ?? 0,
      currentConnections: realTime.currentConnections ?? 0,
      packetsReceived: realTime.packetsReceived ?? 0,
      packetsSent: realTime.packetsSent ?? 0
    }
  }
  return stats.value
})

// 合并活动记录
const displayActivities = computed(() => {
  const realTimeActivities = signalrStore.recentActivities
  if (realTimeActivities.length > 0) {
    // 合并实时活动和 API 活动，去重
    const combined = [...realTimeActivities]
    activities.value.forEach(a => {
      if (!combined.some(r => r.time === a.time && r.message === a.message)) {
        combined.push({ type: a.type, message: a.message, time: a.time })
      }
    })
    return combined.slice(0, 10)
  }
  return activities.value
})

// 在线趋势图表配置
const chartOption = ref({
  tooltip: { trigger: 'axis' },
  xAxis: {
    type: 'category',
    data: [] as string[]
  },
  yAxis: { type: 'value', name: '在线人数' },
  series: [{
    data: [] as number[],
    type: 'line',
    smooth: true,
    areaStyle: { opacity: 0.3 },
    itemStyle: { color: '#18a058' }
  }]
})


// 获取初始数据
async function fetchData() {
  loading.value = true
  try {
    const [statsRes, trendRes, activitiesRes] = await Promise.all([
      dashboardApi.getStats(),
      dashboardApi.getOnlineTrend(24),
      dashboardApi.getActivities(10)
    ])

    if (statsRes.success) stats.value = statsRes.data!
    if (trendRes.success) {
      trendData.value = trendRes.data!
      chartOption.value.xAxis.data = trendRes.data!.map(p =>
        new Date(p.time).toLocaleTimeString('zh-CN', { hour: '2-digit', minute: '2-digit' })
      )
      chartOption.value.series[0].data = trendRes.data!.map(p => p.count)
    }
    if (activitiesRes.success) activities.value = activitiesRes.data!
  } finally {
    loading.value = false
  }
}

// 活动类型映射
const activityTypeMap: Record<string, { type: 'success' | 'warning' | 'error' | 'info'; text: string }> = {
  login: { type: 'success', text: '登录' },
  logout: { type: 'info', text: '下线' },
  register: { type: 'info', text: '注册' },
  order: { type: 'success', text: '订单' },
  block: { type: 'warning', text: '封禁' },
  ban: { type: 'warning', text: '封禁' },
  cheat: { type: 'error', text: '外挂' }
}

// 连接状态颜色
const connectionStatusColor = computed(() => {
  switch (signalrStore.connectionState) {
    case 'connected': return 'success'
    case 'connecting': return 'warning'
    default: return 'error'
  }
})

const connectionStatusText = computed(() => {
  switch (signalrStore.connectionState) {
    case 'connected': return '实时连接'
    case 'connecting': return '连接中...'
    default: return '离线'
  }
})

// 降级轮询 (当 SignalR 不可用时)
let fallbackTimer: number | null = null

function startFallbackPolling() {
  if (!signalrStore.isConnected && !fallbackTimer) {
    fallbackTimer = window.setInterval(fetchData, 30000)
  }
}

function stopFallbackPolling() {
  if (fallbackTimer) {
    clearInterval(fallbackTimer)
    fallbackTimer = null
  }
}

// 监听 SignalR 连接状态
watch(() => signalrStore.isConnected, (connected) => {
  if (connected) {
    stopFallbackPolling()
  } else {
    startFallbackPolling()
  }
})

onMounted(async () => {
  // 1. 先获取初始数据
  await fetchData()

  // 2. 尝试连接 SignalR
  const connected = await signalrStore.connectDashboard()

  // 3. 如果连接失败，启用降级轮询
  if (!connected) {
    startFallbackPolling()
  }
})

onUnmounted(() => {
  stopFallbackPolling()
  // 注意：不在这里断开 SignalR，因为其他页面可能还需要
})
</script>

<template>
  <n-spin :show="loading">
    <!-- 连接状态指示器 -->
    <div class="connection-status">
      <n-tooltip>
        <template #trigger>
          <n-tag :type="connectionStatusColor" size="small" round>
            <template #icon>
              <n-icon :component="signalrStore.isConnected ? WifiOutline : CloudOfflineOutline" />
            </template>
            {{ connectionStatusText }}
          </n-tag>
        </template>
        <div>
          <div v-if="signalrStore.isConnected">数据实时更新中</div>
          <div v-else-if="signalrStore.isConnecting">正在建立连接...</div>
          <div v-else>
            <div>连接断开，每 30 秒自动刷新</div>
            <div v-if="signalrStore.lastError" style="color: #f0a020;">{{ signalrStore.lastError }}</div>
          </div>
        </div>
      </n-tooltip>
    </div>

    <!-- 统计卡片 -->
    <n-grid :cols="4" :x-gap="16" :y-gap="16">
      <n-grid-item>
        <n-card>
          <n-statistic label="总用户数" :value="displayStats?.totalAccounts || 0" />
        </n-card>
      </n-grid-item>
      <n-grid-item>
        <n-card>
          <n-statistic label="今日新增" :value="displayStats?.todayNewAccounts || 0" />
        </n-card>
      </n-grid-item>
      <n-grid-item>
        <n-card>
          <n-badge :value="signalrStore.isConnected ? 'LIVE' : ''" :offset="[-10, 0]" type="success">
            <n-statistic label="当前在线" :value="displayStats?.onlineCount || 0" />
          </n-badge>
        </n-card>
      </n-grid-item>
      <n-grid-item>
        <n-card>
          <n-statistic label="封禁IP数" :value="displayStats?.blockedIpCount || 0" />
        </n-card>
      </n-grid-item>
    </n-grid>

    <!-- 图表区域 -->
    <n-grid :cols="2" :x-gap="16" style="margin-top: 16px">
      <n-grid-item>
        <n-card title="在线趋势 (24小时)">
          <v-chart :option="chartOption" style="height: 300px" autoresize />
        </n-card>
      </n-grid-item>
      <n-grid-item>
        <n-card title="最近活动">
          <n-list v-if="displayActivities.length" :hoverable="true">
            <n-list-item v-for="(activity, index) in displayActivities" :key="index">
              <n-space align="center">
                <n-tag :type="activityTypeMap[activity.type]?.type || 'default'" size="small">
                  {{ activityTypeMap[activity.type]?.text || activity.type }}
                </n-tag>
                <span>{{ activity.message }}</span>
                <span style="color: #999; font-size: 12px;">
                  {{ new Date(activity.time).toLocaleTimeString('zh-CN') }}
                </span>
              </n-space>
            </n-list-item>
          </n-list>
          <n-empty v-else description="暂无活动记录" />
        </n-card>
      </n-grid-item>
    </n-grid>

    <!-- 服务器状态 -->
    <n-grid :cols="3" :x-gap="16" style="margin-top: 16px">
      <n-grid-item>
        <n-card title="服务器状态">
          <n-space vertical size="large">
            <n-space align="center">
              <span>运行状态:</span>
              <n-tag :type="displayStats?.serverStatus === 'running' ? 'success' : 'error'" size="large">
                {{ displayStats?.serverStatus === 'running' ? '运行中' : '已停止' }}
              </n-tag>
            </n-space>
            <n-statistic label="运行时间" :value="displayStats?.uptime || '-'" />
          </n-space>
        </n-card>
      </n-grid-item>
      <n-grid-item :span="2">
        <ResourceChart
          :cpu-usage="displayStats?.cpuUsage"
          :memory-usage-m-b="displayStats?.memoryUsageMB"
          :max-memory-m-b="1024"
          :is-connected="signalrStore.isConnected"
        />
      </n-grid-item>
    </n-grid>

    <!-- 连接和数据包监控 -->
    <n-grid :cols="2" :x-gap="16" style="margin-top: 16px">
      <n-grid-item>
        <ConnectionChart
          :current-connections="displayStats?.currentConnections"
          :online-count="displayStats?.onlineCount"
          :is-connected="signalrStore.isConnected"
        />
      </n-grid-item>
      <n-grid-item>
        <PacketChart
          :packets-received="displayStats?.packetsReceived"
          :packets-sent="displayStats?.packetsSent"
          :is-connected="signalrStore.isConnected"
        />
      </n-grid-item>
    </n-grid>

    <!-- 告警面板 -->
    <n-card title="系统告警" style="margin-top: 16px" v-if="signalrStore.alerts.length > 0">
      <n-list :hoverable="true">
        <n-list-item v-for="(alert, index) in signalrStore.alerts" :key="index">
          <n-space align="center">
            <n-tag :type="alert.type === 'error' ? 'error' : 'warning'" size="small">
              {{ alert.type }}
            </n-tag>
            <span>{{ alert.message }}</span>
            <span style="color: #999; font-size: 12px;">
              {{ new Date(alert.time).toLocaleTimeString('zh-CN') }}
            </span>
          </n-space>
        </n-list-item>
      </n-list>
    </n-card>
  </n-spin>
</template>

<style scoped>
.connection-status {
  position: absolute;
  top: 16px;
  right: 16px;
  z-index: 10;
}
</style>
