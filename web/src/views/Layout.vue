<script setup lang="ts">
import { h, computed } from 'vue'
import { useRouter } from 'vue-router'
import {
  NLayout, NLayoutSider, NLayoutHeader, NLayoutContent,
  NMenu, NIcon, NAvatar, NDropdown, NSwitch, NSpace
} from 'naive-ui'
import type { MenuOption } from 'naive-ui'
import {
  HomeOutline, PeopleOutline, BanOutline, SettingsOutline,
  LogOutOutline, PersonOutline, MoonOutline, SunnyOutline
} from '@vicons/ionicons5'
import { useAuthStore } from '@/stores/auth'
import { useThemeStore } from '@/stores/theme'

const router = useRouter()
const authStore = useAuthStore()
const themeStore = useThemeStore()

const menuOptions: MenuOption[] = [
  {
    label: '仪表盘',
    key: 'Dashboard',
    icon: () => h(NIcon, null, { default: () => h(HomeOutline) })
  },
  {
    label: '玩家管理',
    key: 'Players',
    icon: () => h(NIcon, null, { default: () => h(PeopleOutline) })
  },
  {
    label: 'IP 黑名单',
    key: 'Blacklist',
    icon: () => h(NIcon, null, { default: () => h(BanOutline) })
  },
  {
    label: '系统设置',
    key: 'Settings',
    icon: () => h(NIcon, null, { default: () => h(SettingsOutline) })
  }
]

const userDropdownOptions = [
  {
    label: '个人信息',
    key: 'profile',
    icon: () => h(NIcon, null, { default: () => h(PersonOutline) })
  },
  { type: 'divider', key: 'd1' },
  {
    label: '退出登录',
    key: 'logout',
    icon: () => h(NIcon, null, { default: () => h(LogOutOutline) })
  }
]

const activeKey = computed(() => router.currentRoute.value.name as string)

function handleMenuClick(key: string) {
  router.push({ name: key })
}

async function handleUserAction(key: string) {
  if (key === 'logout') {
    await authStore.logout()
    router.push({ name: 'Login' })
  } else if (key === 'profile') {
    router.push({ name: 'Settings' })
  }
}
</script>

<template>
  <n-layout has-sider style="height: 100vh">
    <n-layout-sider
      bordered
      collapse-mode="width"
      :collapsed-width="64"
      :width="220"
      show-trigger
      content-style="padding: 16px 0;"
    >
      <div class="logo">
        <span class="logo-text">AionNetGate</span>
      </div>
      <n-menu
        :value="activeKey"
        :options="menuOptions"
        @update:value="handleMenuClick"
      />
    </n-layout-sider>

    <n-layout>
      <n-layout-header bordered style="height: 56px; padding: 0 24px;">
        <n-space justify="end" align="center" style="height: 100%">
          <n-switch
            :value="themeStore.isDark"
            @update:value="themeStore.setTheme"
          >
            <template #checked>
              <n-icon :component="MoonOutline" />
            </template>
            <template #unchecked>
              <n-icon :component="SunnyOutline" />
            </template>
          </n-switch>

          <n-dropdown
            :options="userDropdownOptions"
            @select="handleUserAction"
          >
            <n-space align="center" style="cursor: pointer">
              <n-avatar round size="small">
                {{ authStore.user?.username?.charAt(0)?.toUpperCase() || 'U' }}
              </n-avatar>
              <span>{{ authStore.user?.username || '用户' }}</span>
            </n-space>
          </n-dropdown>
        </n-space>
      </n-layout-header>

      <n-layout-content content-style="padding: 24px; background: var(--n-color);">
        <router-view />
      </n-layout-content>
    </n-layout>
  </n-layout>
</template>

<style scoped>
.logo {
  padding: 16px 24px;
  text-align: center;
}

.logo-text {
  font-size: 18px;
  font-weight: bold;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}
</style>
