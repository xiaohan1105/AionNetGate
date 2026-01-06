<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  NCard, NDescriptions, NDescriptionsItem, NTag, NButton, NSpace,
  NSpin, NEmpty, NDataTable, useMessage, useDialog
} from 'naive-ui'
import type { DataTableColumns } from 'naive-ui'
import { playersApi } from '@/api/players'
import type { PlayerDetail, HardwareFingerprint } from '@/api/types'

const route = useRoute()
const router = useRouter()
const message = useMessage()
const dialog = useDialog()

const loading = ref(true)
const player = ref<PlayerDetail | null>(null)

const fingerprintColumns: DataTableColumns<HardwareFingerprint> = [
  { title: 'ID', key: 'id', width: 60 },
  { title: '指纹哈希', key: 'fingerprintHash', width: 200, ellipsis: { tooltip: true } },
  { title: 'CPU ID', key: 'cpuId', width: 150, ellipsis: { tooltip: true } },
  { title: 'MAC 地址', key: 'macAddress', width: 150 },
  { title: '首次使用', key: 'firstSeenAt', width: 180 },
  { title: '最后使用', key: 'lastSeenAt', width: 180 }
]

async function fetchPlayer() {
  const id = Number(route.params.id)
  if (!id) return

  loading.value = true
  try {
    const response = await playersApi.getPlayer(id)
    if (response.success && response.data) {
      player.value = response.data
    } else {
      message.error(response.message || '加载失败')
    }
  } catch (error: any) {
    message.error(error.message || '加载失败')
  } finally {
    loading.value = false
  }
}

async function handleStatusChange(status: number) {
  if (!player.value) return

  dialog.warning({
    title: '确认操作',
    content: `确定要修改玩家状态吗？`,
    positiveText: '确定',
    negativeText: '取消',
    onPositiveClick: async () => {
      try {
        const response = await playersApi.updateStatus(player.value!.id, status)
        if (response.success) {
          message.success('操作成功')
          fetchPlayer()
        } else {
          message.error(response.message)
        }
      } catch (error: any) {
        message.error(error.message || '操作失败')
      }
    }
  })
}

async function handleUnlock() {
  if (!player.value) return

  try {
    const response = await playersApi.unlockAccount(player.value.id)
    if (response.success) {
      message.success('账号已解锁')
      fetchPlayer()
    } else {
      message.error(response.message)
    }
  } catch (error: any) {
    message.error(error.message || '操作失败')
  }
}

async function handleResetPassword() {
  if (!player.value) return

  dialog.warning({
    title: '确认操作',
    content: '确定要重置该玩家的密码吗？',
    positiveText: '确定',
    negativeText: '取消',
    onPositiveClick: async () => {
      try {
        const response = await playersApi.resetPassword(player.value!.id)
        if (response.success && response.data) {
          dialog.success({
            title: '密码已重置',
            content: `新密码: ${response.data}`,
            positiveText: '确定'
          })
        } else {
          message.error(response.message)
        }
      } catch (error: any) {
        message.error(error.message || '操作失败')
      }
    }
  })
}

onMounted(fetchPlayer)
</script>

<template>
  <n-spin :show="loading">
    <n-space vertical v-if="player">
      <n-card title="玩家详情">
        <template #header-extra>
          <n-button @click="router.back()">返回</n-button>
        </template>

        <n-descriptions :column="3" bordered>
          <n-descriptions-item label="ID">{{ player.id }}</n-descriptions-item>
          <n-descriptions-item label="用户名">{{ player.username }}</n-descriptions-item>
          <n-descriptions-item label="邮箱">{{ player.email || '-' }}</n-descriptions-item>
          <n-descriptions-item label="状态">
            <n-tag :type="player.status === 1 ? 'success' : player.status === 2 ? 'warning' : 'error'">
              {{ player.statusText }}
            </n-tag>
          </n-descriptions-item>
          <n-descriptions-item label="角色">
            <n-tag :type="player.role === 99 ? 'error' : player.role === 10 ? 'warning' : 'default'">
              {{ player.roleText }}
            </n-tag>
          </n-descriptions-item>
          <n-descriptions-item label="活跃会话">{{ player.activeSessions }}</n-descriptions-item>
          <n-descriptions-item label="登录失败次数">{{ player.loginAttempts }}</n-descriptions-item>
          <n-descriptions-item label="锁定至">{{ player.lockedUntil || '-' }}</n-descriptions-item>
          <n-descriptions-item label="最后登录IP">{{ player.lastLoginIp || '-' }}</n-descriptions-item>
          <n-descriptions-item label="最后登录时间">{{ player.lastLoginAt || '-' }}</n-descriptions-item>
          <n-descriptions-item label="注册时间">{{ player.createdAt }}</n-descriptions-item>
          <n-descriptions-item label="更新时间">{{ player.updatedAt }}</n-descriptions-item>
        </n-descriptions>

        <n-space style="margin-top: 16px">
          <n-button v-if="player.status !== 1" type="success" @click="handleStatusChange(1)">启用账号</n-button>
          <n-button v-if="player.status === 1" type="warning" @click="handleStatusChange(0)">禁用账号</n-button>
          <n-button v-if="player.lockedUntil" type="info" @click="handleUnlock">解锁账号</n-button>
          <n-button type="error" @click="handleResetPassword">重置密码</n-button>
        </n-space>
      </n-card>

      <n-card title="硬件指纹">
        <n-data-table
          :columns="fingerprintColumns"
          :data="player.hardwareFingerprints"
          :row-key="(row: HardwareFingerprint) => row.id"
          :scroll-x="900"
        />
      </n-card>
    </n-space>

    <n-empty v-else-if="!loading" description="玩家不存在" />
  </n-spin>
</template>
