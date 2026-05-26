<script setup lang="ts">
import accessIcon from '@/assets/icons/access.svg'
import type { SubmissionStatus } from '@/entities/submission/model/types'

defineProps<{
  status: SubmissionStatus
}>()
</script>

<template>
  <span v-if="status === 'graded'" class="badge badge--ok">
    Проверено <img :src="accessIcon" alt="" class="badge-ico">
  </span>
  <span v-else class="badge badge--pend">На проверке</span>
</template>

<style scoped>
.badge {
  display: inline-flex;
  align-items: center;
  gap: 0.3rem;
  border-radius: 999px;
  padding: 0.2rem 0.7rem;
  font-size: 0.75rem;
  font-weight: 600;
  color: #fff;
}
.badge--ok {
  background: #22c55e;
}
.badge--pend {
  background: rgba(255, 255, 255, 0.2);
  padding: 0.2rem 0.65rem;
}
.badge-ico {
  width: 13px;
  height: 13px;
}
</style>
