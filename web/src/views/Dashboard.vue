<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import {
  NGrid, NGridItem, NCard, NStatistic, NSpace, NSpin, NList, NListItem,
  NTag, NEmpty
} from 'naive-ui'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { LineChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, LegendComponent } from 'echarts/components'
import VChart from 'vue-echarts'
import { dashboardApi } from '@/api/dashboard'
import type { DashboardStats, OnlineTrendPoint, ActivityLog } from '@/api/types'

use([CanvasRenderer, LineChart, GridComponent, TooltipComponent, LegendComponent])

const loading = ref(true)
const stats = ref<DashboardStats | null>(null)
const trendData = ref<OnlineTrendPoint[]>([])
const activities = ref<ActivityLog[]>([])

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
    areaStyle: { opacity: 0.3 }
  }]
})

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

const activityTypeMap: Record<string, { type: 'success' | 'warning' | 'error' | 'info'; text: string }> = {
  login: { type: 'success', text: '登录' },
  register: { type: 'info', text: '注册' },
  order: { type: 'success', text: '订单' },
  block: { type: 'warning', text: '封禁' },
  cheat: { type: 'error', text: '外挂' }
}

let refreshTimer: number

onMounted(() => {
  fetchData()
  refreshTimer = window.setInterval(fetchData, 30000)
})

onUnmounted(() => {
  if (refreshTimer) clearInterval(refreshTimer)
})
</script>

<template>
  <n-spin :show="loading">
    <n-grid :cols="4" :x-gap="16" :y-gap="16">
      <n-grid-item>
        <n-card>
          <n-statistic label="总用户数" :value="stats?.totalAccounts || 0" />
        </n-card>
      </n-grid-item>
      <n-grid-item>
        <n-card>
          <n-statistic label="今日新增" :value="stats?.todayNewAccounts || 0" />
        </n-card>
      </n-grid-item>
      <n-grid-item>
        <n-card>
          <n-statistic label="当前在线" :value="stats?.onlineCount || 0" />
        </n-card>
      </n-grid-item>
      <n-grid-item>
        <n-card>
          <n-statistic label="封禁IP数" :value="stats?.blockedIpCount || 0" />
        </n-card>
      </n-grid-item>
    </n-grid>

    <n-grid :cols="2" :x-gap="16" style="margin-top: 16px">
      <n-grid-item>
        <n-card title="在线趋势 (24小时)">
          <v-chart :option="chartOption" style="height: 300px" autoresize />
        </n-card>
      </n-grid-item>
      <n-grid-item>
        <n-card title="最近活动">
          <n-list v-if="activities.length">
            <n-list-item v-for="activity in activities" :key="activity.id">
              <n-space align="center">
                <n-tag :type="activityTypeMap[activity.type]?.type || 'default'" size="small">
                  {{ activityTypeMap[activity.type]?.text || activity.type }}
                </n-tag>
                <span>{{ activity.message }}</span>
              </n-space>
            </n-list-item>
          </n-list>
          <n-empty v-else description="暂无活动记录" />
        </n-card>
      </n-grid-item>
    </n-grid>

    <n-card title="服务器状态" style="margin-top: 16px">
      <n-space>
        <n-tag :type="stats?.serverStatus === 'running' ? 'success' : 'error'">
          {{ stats?.serverStatus === 'running' ? '运行中' : '已停止' }}
        </n-tag>
        <span>运行时间: {{ stats?.uptime || '-' }}</span>
      </n-space>
    </n-card>
  </n-spin>
</template>
