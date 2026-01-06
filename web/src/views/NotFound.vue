<script setup lang="ts">
import { NResult, NButton } from 'naive-ui'
import { useRouter } from 'vue-router'

const router = useRouter()
</script>

<template>
  <div class="not-found">
    <n-result status="404" title="404" description="页面不存在">
      <template #footer>
        <n-button @click="router.push('/')">返回首页</n-button>
      </template>
    </n-result>
  </div>
</template>

<style scoped>
.not-found {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
}
</style>
