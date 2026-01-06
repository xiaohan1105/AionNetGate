<script setup lang="ts">
import { ref, onMounted, h } from 'vue'
import { useRouter } from 'vue-router'
import {
  NDataTable, NCard, NInput, NSelect, NSpace, NButton, NTag, NPagination,
  useMessage, useDialog, NIcon
} from 'naive-ui'
import type { DataTableColumns } from 'naive-ui'
import { SearchOutline, RefreshOutline } from '@vicons/ionicons5'
import { playersApi } from '@/api/players'
import type { Player, PagedResponse } from '@/api/types'

const router = useRouter()
const message = useMessage()
const dialog = useDialog()

const loading = ref(false)
const data = ref<Player[]>([])
const pagination = ref({
  page: 1,
  pageSize: 20,
  itemCount: 0
})
const searchText = ref('')
const statusFilter = ref<number | null>(null)

const statusOptions = [
  { label: '全部状态', value: null },
  { label: '正常', value: 1 },
  { label: '禁用', value: 0 },
  { label: '锁定', value: 2 }
]

const columns: DataTableColumns<Player> = [
  { title: 'ID', key: 'id', width: 80 },
  { title: '用户名', key: 'username', width: 150 },
  { title: '邮箱', key: 'email', width: 200, ellipsis: { tooltip: true } },
  {
    title: '状态',
    key: 'status',
    width: 100,
    render: (row) => h(NTag, {
      type: row.status === 1 ? 'success' : row.status === 2 ? 'warning' : 'error',
      size: 'small'
    }, { default: () => row.statusText })
  },
  {
    title: '角色',
    key: 'role',
    width: 100,
    render: (row) => h(NTag, {
      type: row.role === 99 ? 'error' : row.role === 10 ? 'warning' : 'default',
      size: 'small'
    }, { default: () => row.roleText })
  },
  { title: '最后登录', key: 'lastLoginAt', width: 180 },
  { title: '最后登录IP', key: 'lastLoginIp', width: 140 },
  { title: '注册时间', key: 'createdAt', width: 180 },
  {
    title: '操作',
    key: 'actions',
    width: 120,
    render: (row) => h(NSpace, null, {
      default: () => [
        h(NButton, {
          size: 'small',
          onClick: () => router.push({ name: 'PlayerDetail', params: { id: row.id } })
        }, { default: () => '详情' }),
        h(NButton, {
          size: 'small',
          type: 'warning',
          onClick: () => handleKick(row)
        }, { default: () => '踢出' })
      ]
    })
  }
]

async function fetchData() {
  loading.value = true
  try {
    const response = await playersApi.getPlayers({
      page: pagination.value.page,
      pageSize: pagination.value.pageSize,
      search: searchText.value || undefined,
      status: statusFilter.value ?? undefined
    })
    if (response.success && response.data) {
      data.value = response.data.items
      pagination.value.itemCount = response.data.totalCount
    }
  } catch (error: any) {
    message.error(error.message || '加载失败')
  } finally {
    loading.value = false
  }
}

function handlePageChange(page: number) {
  pagination.value.page = page
  fetchData()
}

function handleSearch() {
  pagination.value.page = 1
  fetchData()
}

async function handleKick(player: Player) {
  dialog.warning({
    title: '确认操作',
    content: `确定要踢出玩家 ${player.username} 吗？`,
    positiveText: '确定',
    negativeText: '取消',
    onPositiveClick: async () => {
      try {
        const response = await playersApi.kickPlayer(player.id)
        if (response.success) {
          message.success('操作成功')
          fetchData()
        } else {
          message.error(response.message)
        }
      } catch (error: any) {
        message.error(error.message || '操作失败')
      }
    }
  })
}

onMounted(fetchData)
</script>

<template>
  <n-card title="玩家管理">
    <template #header-extra>
      <n-space>
        <n-input
          v-model:value="searchText"
          placeholder="搜索用户名/邮箱"
          clearable
          style="width: 200px"
          @keyup.enter="handleSearch"
        >
          <template #prefix>
            <n-icon :component="SearchOutline" />
          </template>
        </n-input>
        <n-select
          v-model:value="statusFilter"
          :options="statusOptions"
          style="width: 120px"
          @update:value="handleSearch"
        />
        <n-button @click="fetchData">
          <template #icon>
            <n-icon :component="RefreshOutline" />
          </template>
        </n-button>
      </n-space>
    </template>

    <n-data-table
      :columns="columns"
      :data="data"
      :loading="loading"
      :row-key="(row: Player) => row.id"
      :pagination="false"
      :scroll-x="1200"
    />

    <n-space justify="end" style="margin-top: 16px">
      <n-pagination
        v-model:page="pagination.page"
        :page-size="pagination.pageSize"
        :item-count="pagination.itemCount"
        show-size-picker
        :page-sizes="[10, 20, 50, 100]"
        @update:page="handlePageChange"
        @update:page-size="(size: number) => { pagination.pageSize = size; handleSearch() }"
      />
    </n-space>
  </n-card>
</template>
