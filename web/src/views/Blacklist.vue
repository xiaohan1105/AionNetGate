<script setup lang="ts">
import { ref, reactive, onMounted, computed, h } from 'vue'
import {
  NCard, NDataTable, NButton, NSpace, NInput, NSelect, NPagination,
  NModal, NForm, NFormItem, NInputNumber, NSwitch, NTag, NIcon,
  NPopconfirm, useMessage, NEmpty
} from 'naive-ui'
import { AddOutline, TrashOutline, RefreshOutline, SearchOutline } from '@vicons/ionicons5'
import { blacklistApi } from '@/api/blacklist'
import type { BlacklistEntry, AddBlacklistRequest, BlacklistQuery } from '@/api/blacklist'
import type { DataTableColumns, PaginationProps } from 'naive-ui'

const message = useMessage()

// 状态
const loading = ref(false)
const data = ref<BlacklistEntry[]>([])
const totalCount = ref(0)

// 查询条件
const query = reactive<BlacklistQuery>({
  page: 1,
  pageSize: 20,
  search: '',
  isActive: undefined
})

// 新增模态框
const showAddModal = ref(false)
const addForm = reactive<AddBlacklistRequest>({
  ipAddress: '',
  reason: '',
  duration: undefined
})
const isPermanent = ref(true)
const adding = ref(false)

// 活跃状态过滤选项
const activeOptions = [
  { label: '全部', value: undefined },
  { label: '生效中', value: true },
  { label: '已过期', value: false }
]

// 表格列定义
const columns: DataTableColumns<BlacklistEntry> = [
  {
    title: 'IP 地址',
    key: 'ipAddress',
    width: 150
  },
  {
    title: '封禁原因',
    key: 'reason',
    ellipsis: { tooltip: true }
  },
  {
    title: '类型',
    key: 'isPermanent',
    width: 100,
    render: (row) => h(NTag, {
      type: row.isPermanent ? 'error' : 'warning',
      size: 'small'
    }, () => row.isPermanent ? '永久' : '临时')
  },
  {
    title: '状态',
    key: 'isActive',
    width: 80,
    render: (row) => h(NTag, {
      type: row.isActive ? 'success' : 'default',
      size: 'small'
    }, () => row.isActive ? '生效' : '过期')
  },
  {
    title: '创建时间',
    key: 'createdAt',
    width: 180,
    render: (row) => new Date(row.createdAt).toLocaleString('zh-CN')
  },
  {
    title: '过期时间',
    key: 'expiresAt',
    width: 180,
    render: (row) => row.expiresAt
      ? new Date(row.expiresAt).toLocaleString('zh-CN')
      : h('span', { style: 'color: #999' }, '永不过期')
  },
  {
    title: '操作',
    key: 'actions',
    width: 100,
    render: (row) => h(NPopconfirm, {
      onPositiveClick: () => handleRemove(row.id)
    }, {
      trigger: () => h(NButton, {
        size: 'small',
        type: 'error',
        quaternary: true
      }, {
        icon: () => h(NIcon, { component: TrashOutline }),
        default: () => '移除'
      }),
      default: () => '确定要移除此IP吗？'
    })
  }
]

// 加载数据
async function loadData() {
  loading.value = true
  try {
    const res = await blacklistApi.getList(query)
    if (res.success && res.data) {
      data.value = res.data.items || []
      totalCount.value = res.data.totalCount || 0
    }
  } catch (err) {
    message.error('加载失败')
  } finally {
    loading.value = false
  }
}

// 搜索
function handleSearch() {
  query.page = 1
  loadData()
}

// 分页变化
function handlePageChange(page: number) {
  query.page = page
  loadData()
}

function handlePageSizeChange(pageSize: number) {
  query.pageSize = pageSize
  query.page = 1
  loadData()
}

// 打开新增模态框
function openAddModal() {
  addForm.ipAddress = ''
  addForm.reason = ''
  addForm.duration = undefined
  isPermanent.value = true
  showAddModal.value = true
}

// 新增
async function handleAdd() {
  if (!addForm.ipAddress) {
    message.warning('请输入IP地址')
    return
  }

  // IP格式验证
  const ipRegex = /^(\d{1,3}\.){3}\d{1,3}$/
  if (!ipRegex.test(addForm.ipAddress)) {
    message.warning('请输入有效的IP地址')
    return
  }

  adding.value = true
  try {
    const request: AddBlacklistRequest = {
      ipAddress: addForm.ipAddress,
      reason: addForm.reason || undefined,
      duration: isPermanent.value ? undefined : (addForm.duration || 60)
    }

    const res = await blacklistApi.add(request)
    if (res.success) {
      message.success('添加成功')
      showAddModal.value = false
      loadData()
    } else {
      message.error(res.message || '添加失败')
    }
  } catch (err) {
    message.error('添加失败')
  } finally {
    adding.value = false
  }
}

// 移除
async function handleRemove(id: number) {
  try {
    const res = await blacklistApi.remove(id)
    if (res.success) {
      message.success('移除成功')
      loadData()
    } else {
      message.error(res.message || '移除失败')
    }
  } catch (err) {
    message.error('移除失败')
  }
}

// 清理过期
async function handleCleanup() {
  try {
    const res = await blacklistApi.cleanup()
    if (res.success) {
      message.success('清理完成')
      loadData()
    } else {
      message.error(res.message || '清理失败')
    }
  } catch (err) {
    message.error('清理失败')
  }
}

// 分页信息
const pagination = computed<PaginationProps>(() => ({
  page: query.page,
  pageSize: query.pageSize,
  pageCount: Math.ceil(totalCount.value / (query.pageSize || 20)),
  itemCount: totalCount.value,
  showSizePicker: true,
  pageSizes: [10, 20, 50, 100],
  prefix: () => `共 ${totalCount.value} 条`
}))

onMounted(() => {
  loadData()
})
</script>

<template>
  <n-card title="IP 黑名单">
    <!-- 工具栏 -->
    <template #header-extra>
      <n-space>
        <n-button type="primary" @click="openAddModal">
          <template #icon><n-icon :component="AddOutline" /></template>
          添加
        </n-button>
        <n-popconfirm @positive-click="handleCleanup">
          <template #trigger>
            <n-button>
              <template #icon><n-icon :component="TrashOutline" /></template>
              清理过期
            </n-button>
          </template>
          确定要清理所有过期记录吗？
        </n-popconfirm>
        <n-button @click="loadData">
          <template #icon><n-icon :component="RefreshOutline" /></template>
          刷新
        </n-button>
      </n-space>
    </template>

    <!-- 搜索栏 -->
    <n-space style="margin-bottom: 16px">
      <n-input
        v-model:value="query.search"
        placeholder="搜索IP地址或原因..."
        clearable
        style="width: 250px"
        @keyup.enter="handleSearch"
      >
        <template #prefix>
          <n-icon :component="SearchOutline" />
        </template>
      </n-input>
      <n-select
        v-model:value="query.isActive"
        :options="activeOptions"
        placeholder="状态"
        style="width: 120px"
        @update:value="handleSearch"
      />
      <n-button @click="handleSearch">搜索</n-button>
    </n-space>

    <!-- 数据表格 -->
    <n-data-table
      :columns="columns"
      :data="data"
      :loading="loading"
      :bordered="false"
      :single-line="false"
      striped
    />

    <!-- 分页 -->
    <n-space justify="end" style="margin-top: 16px">
      <n-pagination
        v-model:page="query.page"
        v-model:page-size="query.pageSize"
        :page-count="pagination.pageCount"
        :item-count="pagination.itemCount"
        :page-sizes="[10, 20, 50, 100]"
        show-size-picker
        @update:page="handlePageChange"
        @update:page-size="handlePageSizeChange"
      >
        <template #prefix>共 {{ totalCount }} 条</template>
      </n-pagination>
    </n-space>

    <!-- 空状态 -->
    <n-empty v-if="!loading && data.length === 0" description="暂无黑名单记录" style="margin-top: 40px" />

    <!-- 新增模态框 -->
    <n-modal
      v-model:show="showAddModal"
      title="添加IP到黑名单"
      preset="dialog"
      style="width: 450px"
    >
      <n-form label-placement="left" label-width="100">
        <n-form-item label="IP 地址" required>
          <n-input v-model:value="addForm.ipAddress" placeholder="例如: 192.168.1.100" />
        </n-form-item>
        <n-form-item label="封禁原因">
          <n-input v-model:value="addForm.reason" placeholder="可选，例如: 恶意攻击" />
        </n-form-item>
        <n-form-item label="永久封禁">
          <n-switch v-model:value="isPermanent" />
        </n-form-item>
        <n-form-item v-if="!isPermanent" label="封禁时长(分钟)">
          <n-input-number
            v-model:value="addForm.duration"
            :min="1"
            :max="525600"
            placeholder="默认 60 分钟"
            style="width: 100%"
          />
        </n-form-item>
      </n-form>
      <template #action>
        <n-space justify="end">
          <n-button @click="showAddModal = false">取消</n-button>
          <n-button type="primary" :loading="adding" @click="handleAdd">确定</n-button>
        </n-space>
      </template>
    </n-modal>
  </n-card>
</template>
