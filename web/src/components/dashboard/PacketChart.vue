<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { NCard, NStatistic, NSpace, NTag, NIcon } from 'naive-ui'
import { ArrowUpOutline, ArrowDownOutline } from '@vicons/ionicons5'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { LineChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, LegendComponent } from 'echarts/components'
import VChart from 'vue-echarts'

use([CanvasRenderer, LineChart, GridComponent, TooltipComponent, LegendComponent])

const props = defineProps<{
  packetsReceived?: number
  packetsSent?: number
  isConnected?: boolean
}>()

// 历史数据
const receivedHistory = ref<number[]>([])
const sentHistory = ref<number[]>([])
const timeLabels = ref<string[]>([])

// 上一次的值，用于计算增量
const lastReceived = ref(0)
const lastSent = ref(0)

// 速率（每秒）
const receiveRate = ref(0)
const sendRate = ref(0)

// 更新历史数据
watch(() => [props.packetsReceived, props.packetsSent], ([received, sent]) => {
  if (received !== undefined && props.isConnected) {
    // 计算增量（假设每5秒更新一次）
    const delta = received - lastReceived.value
    receiveRate.value = Math.round(delta / 5)
    lastReceived.value = received

    receivedHistory.value.push(receiveRate.value)
    if (receivedHistory.value.length > 60) receivedHistory.value.shift()
  }

  if (sent !== undefined && props.isConnected) {
    const delta = sent - lastSent.value
    sendRate.value = Math.round(delta / 5)
    lastSent.value = sent

    sentHistory.value.push(sendRate.value)
    if (sentHistory.value.length > 60) sentHistory.value.shift()
  }

  const now = new Date().toLocaleTimeString('zh-CN', {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  })
  timeLabels.value.push(now)
  if (timeLabels.value.length > 60) timeLabels.value.shift()
}, { immediate: true })

// 格式化大数字
function formatNumber(num: number): string {
  if (num >= 1000000) return (num / 1000000).toFixed(2) + 'M'
  if (num >= 1000) return (num / 1000).toFixed(1) + 'K'
  return num.toString()
}

// 图表配置
const chartOption = computed(() => ({
  tooltip: {
    trigger: 'axis',
    formatter: (params: any) => {
      let result = params[0]?.axisValue + '<br/>'
      params.forEach((item: any) => {
        result += `${item.marker} ${item.seriesName}: ${item.value} pkt/s<br/>`
      })
      return result
    }
  },
  legend: {
    data: ['接收速率', '发送速率'],
    bottom: 0
  },
  grid: {
    left: '3%',
    right: '4%',
    bottom: '15%',
    top: '5%',
    containLabel: true
  },
  xAxis: {
    type: 'category',
    boundaryGap: false,
    data: timeLabels.value,
    axisLabel: {
      fontSize: 10,
      rotate: 30
    }
  },
  yAxis: {
    type: 'value',
    name: 'pkt/s',
    minInterval: 1
  },
  series: [
    {
      name: '接收速率',
      type: 'line',
      smooth: true,
      data: receivedHistory.value,
      areaStyle: { opacity: 0.3 },
      itemStyle: { color: '#18a058' },
      emphasis: { focus: 'series' }
    },
    {
      name: '发送速率',
      type: 'line',
      smooth: true,
      data: sentHistory.value,
      areaStyle: { opacity: 0.3 },
      itemStyle: { color: '#2080f0' },
      emphasis: { focus: 'series' }
    }
  ]
}))
</script>

<template>
  <n-card title="数据包统计">
    <template v-if="isConnected">
      <n-space justify="space-around" style="margin-bottom: 16px">
        <n-statistic label="总接收">
          <template #prefix>
            <n-icon :component="ArrowDownOutline" color="#18a058" />
          </template>
          {{ formatNumber(packetsReceived ?? 0) }}
        </n-statistic>
        <n-statistic label="总发送">
          <template #prefix>
            <n-icon :component="ArrowUpOutline" color="#2080f0" />
          </template>
          {{ formatNumber(packetsSent ?? 0) }}
        </n-statistic>
        <n-statistic label="接收速率">
          <template #suffix>/s</template>
          {{ receiveRate }}
        </n-statistic>
        <n-statistic label="发送速率">
          <template #suffix>/s</template>
          {{ sendRate }}
        </n-statistic>
      </n-space>
      <v-chart :option="chartOption" style="height: 200px" autoresize />
    </template>
    <template v-else>
      <div class="offline-placeholder">
        <p>连接 SignalR 后显示数据包统计</p>
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
