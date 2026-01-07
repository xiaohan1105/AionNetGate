<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { NCard, NSpace, NStatistic, NProgress } from 'naive-ui'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { LineChart, GaugeChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, LegendComponent } from 'echarts/components'
import VChart from 'vue-echarts'

use([CanvasRenderer, LineChart, GaugeChart, GridComponent, TooltipComponent, LegendComponent])

interface ResourceData {
  cpuUsage: number
  memoryUsageMB: number
  timestamp: string
}

const props = defineProps<{
  cpuUsage?: number
  memoryUsageMB?: number
  maxMemoryMB?: number
  isConnected?: boolean
}>()

// 历史数据（最近60个点，5秒/点 = 5分钟）
const cpuHistory = ref<number[]>([])
const memoryHistory = ref<number[]>([])
const timeLabels = ref<string[]>([])

// 更新历史数据
watch(() => [props.cpuUsage, props.memoryUsageMB], ([cpu, mem]) => {
  if (cpu !== undefined && props.isConnected) {
    cpuHistory.value.push(cpu)
    if (cpuHistory.value.length > 60) cpuHistory.value.shift()
  }
  if (mem !== undefined && props.isConnected) {
    memoryHistory.value.push(mem)
    if (memoryHistory.value.length > 60) memoryHistory.value.shift()
  }

  const now = new Date().toLocaleTimeString('zh-CN', {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  })
  timeLabels.value.push(now)
  if (timeLabels.value.length > 60) timeLabels.value.shift()
}, { immediate: true })

// CPU 仪表盘配置
const cpuGaugeOption = computed(() => ({
  series: [{
    type: 'gauge',
    startAngle: 180,
    endAngle: 0,
    min: 0,
    max: 100,
    splitNumber: 5,
    radius: '100%',
    center: ['50%', '75%'],
    axisLine: {
      lineStyle: {
        width: 6,
        color: [
          [0.3, '#67C23A'],
          [0.7, '#E6A23C'],
          [1, '#F56C6C']
        ]
      }
    },
    pointer: {
      icon: 'path://M12.8,0.7l12,40.1H0.7L12.8,0.7z',
      length: '50%',
      width: 8,
      offsetCenter: [0, '-15%'],
      itemStyle: { color: 'auto' }
    },
    axisTick: { show: false },
    splitLine: { show: false },
    axisLabel: {
      distance: -25,
      fontSize: 10,
      color: '#999'
    },
    title: {
      offsetCenter: [0, '10%'],
      fontSize: 12,
      color: '#666'
    },
    detail: {
      fontSize: 20,
      offsetCenter: [0, '35%'],
      valueAnimation: true,
      formatter: '{value}%',
      color: 'auto'
    },
    data: [{ value: props.cpuUsage ?? 0, name: 'CPU' }]
  }]
}))

// 内存仪表盘配置
const memoryGaugeOption = computed(() => {
  const maxMem = props.maxMemoryMB || 1024
  const usedPercent = Math.min(((props.memoryUsageMB ?? 0) / maxMem) * 100, 100)

  return {
    series: [{
      type: 'gauge',
      startAngle: 180,
      endAngle: 0,
      min: 0,
      max: 100,
      splitNumber: 5,
      radius: '100%',
      center: ['50%', '75%'],
      axisLine: {
        lineStyle: {
          width: 6,
          color: [
            [0.5, '#67C23A'],
            [0.8, '#E6A23C'],
            [1, '#F56C6C']
          ]
        }
      },
      pointer: {
        icon: 'path://M12.8,0.7l12,40.1H0.7L12.8,0.7z',
        length: '50%',
        width: 8,
        offsetCenter: [0, '-15%'],
        itemStyle: { color: 'auto' }
      },
      axisTick: { show: false },
      splitLine: { show: false },
      axisLabel: {
        distance: -25,
        fontSize: 10,
        color: '#999'
      },
      title: {
        offsetCenter: [0, '10%'],
        fontSize: 12,
        color: '#666'
      },
      detail: {
        fontSize: 16,
        offsetCenter: [0, '35%'],
        valueAnimation: true,
        formatter: () => `${props.memoryUsageMB ?? 0} MB`,
        color: 'auto'
      },
      data: [{ value: usedPercent, name: '内存' }]
    }]
  }
})

// 趋势图配置
const trendChartOption = computed(() => ({
  tooltip: {
    trigger: 'axis',
    axisPointer: { type: 'cross' }
  },
  legend: {
    data: ['CPU (%)', '内存 (MB)'],
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
    axisLabel: { fontSize: 10 }
  },
  yAxis: [
    {
      type: 'value',
      name: 'CPU %',
      min: 0,
      max: 100,
      position: 'left',
      axisLabel: { formatter: '{value}%' }
    },
    {
      type: 'value',
      name: '内存 MB',
      min: 0,
      position: 'right',
      axisLabel: { formatter: '{value}' }
    }
  ],
  series: [
    {
      name: 'CPU (%)',
      type: 'line',
      smooth: true,
      yAxisIndex: 0,
      data: cpuHistory.value,
      areaStyle: { opacity: 0.2 },
      itemStyle: { color: '#18a058' }
    },
    {
      name: '内存 (MB)',
      type: 'line',
      smooth: true,
      yAxisIndex: 1,
      data: memoryHistory.value,
      areaStyle: { opacity: 0.2 },
      itemStyle: { color: '#2080f0' }
    }
  ]
}))
</script>

<template>
  <n-card title="资源监控">
    <template v-if="isConnected">
      <div class="gauges-container">
        <div class="gauge-item">
          <v-chart :option="cpuGaugeOption" style="height: 120px" autoresize />
        </div>
        <div class="gauge-item">
          <v-chart :option="memoryGaugeOption" style="height: 120px" autoresize />
        </div>
      </div>
      <v-chart :option="trendChartOption" style="height: 200px; margin-top: 16px" autoresize />
    </template>
    <template v-else>
      <div class="offline-placeholder">
        <p>连接 SignalR 后显示实时监控数据</p>
      </div>
    </template>
  </n-card>
</template>

<style scoped>
.gauges-container {
  display: flex;
  justify-content: space-around;
}

.gauge-item {
  flex: 1;
  max-width: 200px;
}

.offline-placeholder {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 200px;
  color: #999;
}
</style>
