<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { NCard, NStatistic, NSpace, NTag } from 'naive-ui'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { LineChart, BarChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, LegendComponent, MarkLineComponent } from 'echarts/components'
import VChart from 'vue-echarts'

use([CanvasRenderer, LineChart, BarChart, GridComponent, TooltipComponent, LegendComponent, MarkLineComponent])

const props = defineProps<{
  currentConnections?: number
  onlineCount?: number
  isConnected?: boolean
}>()

// 历史数据（最近60个点）
const connectionHistory = ref<number[]>([])
const onlineHistory = ref<number[]>([])
const timeLabels = ref<string[]>([])

// 统计数据
const stats = computed(() => {
  const connections = connectionHistory.value
  if (connections.length === 0) {
    return { max: 0, min: 0, avg: 0 }
  }
  return {
    max: Math.max(...connections),
    min: Math.min(...connections),
    avg: Math.round(connections.reduce((a, b) => a + b, 0) / connections.length)
  }
})

// 更新历史数据
watch(() => [props.currentConnections, props.onlineCount], ([conn, online]) => {
  if (conn !== undefined && props.isConnected) {
    connectionHistory.value.push(conn)
    if (connectionHistory.value.length > 60) connectionHistory.value.shift()
  }
  if (online !== undefined && props.isConnected) {
    onlineHistory.value.push(online)
    if (onlineHistory.value.length > 60) onlineHistory.value.shift()
  }

  const now = new Date().toLocaleTimeString('zh-CN', {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  })
  timeLabels.value.push(now)
  if (timeLabels.value.length > 60) timeLabels.value.shift()
}, { immediate: true })

// 图表配置
const chartOption = computed(() => ({
  tooltip: {
    trigger: 'axis',
    axisPointer: { type: 'shadow' }
  },
  legend: {
    data: ['TCP 连接数', '在线玩家'],
    bottom: 0
  },
  grid: {
    left: '3%',
    right: '4%',
    bottom: '15%',
    top: '10%',
    containLabel: true
  },
  xAxis: {
    type: 'category',
    data: timeLabels.value,
    axisLabel: {
      fontSize: 10,
      rotate: 30
    }
  },
  yAxis: {
    type: 'value',
    name: '连接数',
    minInterval: 1
  },
  series: [
    {
      name: 'TCP 连接数',
      type: 'line',
      smooth: true,
      data: connectionHistory.value,
      areaStyle: { opacity: 0.3 },
      itemStyle: { color: '#f0a020' },
      markLine: {
        silent: true,
        data: [
          { type: 'average', name: '平均值' }
        ],
        label: {
          formatter: '平均: {c}'
        }
      }
    },
    {
      name: '在线玩家',
      type: 'bar',
      data: onlineHistory.value,
      itemStyle: { color: '#18a058' },
      barWidth: '40%'
    }
  ]
}))
</script>

<template>
  <n-card title="连接监控">
    <template v-if="isConnected">
      <n-space justify="space-around" style="margin-bottom: 16px">
        <n-statistic label="当前连接">
          <template #prefix>
            <n-tag type="warning" size="small">TCP</n-tag>
          </template>
          {{ currentConnections ?? 0 }}
        </n-statistic>
        <n-statistic label="在线玩家">
          <template #prefix>
            <n-tag type="success" size="small">LIVE</n-tag>
          </template>
          {{ onlineCount ?? 0 }}
        </n-statistic>
        <n-statistic label="峰值" :value="stats.max" />
        <n-statistic label="平均" :value="stats.avg" />
      </n-space>
      <v-chart :option="chartOption" style="height: 250px" autoresize />
    </template>
    <template v-else>
      <div class="offline-placeholder">
        <p>连接 SignalR 后显示实时连接数据</p>
      </div>
    </template>
  </n-card>
</template>

<style scoped>
.offline-placeholder {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 200px;
  color: #999;
}
</style>
