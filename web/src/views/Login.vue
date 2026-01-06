<script setup lang="ts">
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useMessage, NCard, NForm, NFormItem, NInput, NButton, NSpace } from 'naive-ui'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const route = useRoute()
const message = useMessage()
const authStore = useAuthStore()

const loading = ref(false)
const formData = ref({
  username: '',
  password: ''
})

async function handleLogin() {
  if (!formData.value.username || !formData.value.password) {
    message.warning('请输入用户名和密码')
    return
  }

  loading.value = true
  try {
    const response = await authStore.login(formData.value.username, formData.value.password)
    if (response.success) {
      message.success('登录成功')
      const redirect = (route.query.redirect as string) || '/'
      router.push(redirect)
    } else {
      message.error(response.message || '登录失败')
    }
  } catch (error: any) {
    message.error(error.message || '登录失败')
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="login-container">
    <n-card title="AionNetGate 管理后台" class="login-card">
      <n-form @submit.prevent="handleLogin">
        <n-form-item label="用户名">
          <n-input
            v-model:value="formData.username"
            placeholder="请输入用户名"
            :disabled="loading"
          />
        </n-form-item>
        <n-form-item label="密码">
          <n-input
            v-model:value="formData.password"
            type="password"
            placeholder="请输入密码"
            show-password-on="click"
            :disabled="loading"
            @keyup.enter="handleLogin"
          />
        </n-form-item>
        <n-space justify="center">
          <n-button
            type="primary"
            size="large"
            :loading="loading"
            @click="handleLogin"
          >
            登录
          </n-button>
        </n-space>
      </n-form>
    </n-card>
  </div>
</template>

<style scoped>
.login-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.login-card {
  width: 400px;
  border-radius: 8px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.2);
}
</style>
