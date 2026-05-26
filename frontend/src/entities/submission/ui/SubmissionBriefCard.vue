<script setup lang="ts">
import type { Submission } from '@/entities/submission/model/types'
import { fmtDate } from '@/shared/lib/date'
import SubmissionStatusBadge from '@/entities/submission/ui/SubmissionStatusBadge.vue'
import SubmissionScoreBlock from '@/entities/submission/ui/SubmissionScoreBlock.vue'

defineProps<{
  submission: Submission
}>()

const emit = defineEmits<{
  detail: []
}>()
</script>

<template>
  <article class="brief-card">
    <div class="brief-head">
      <span class="brief-num">Дз №{{ submission.id }}</span>
      <SubmissionStatusBadge :status="submission.status" />
    </div>
    <p class="brief-date">Сдано: {{ fmtDate(submission.submitted_at) }}</p>
    <p class="brief-title">{{ submission.homework?.title }}</p>
    <SubmissionScoreBlock :submission="submission" />
    <button type="button" class="brief-more" @click="emit('detail')">
      Подробнее
    </button>
  </article>
</template>

<style scoped>
.brief-card {
  min-width: 100%;
  background: rgba(255, 255, 255, 0.13);
  backdrop-filter: blur(12px);
  border: 1px solid rgba(255, 255, 255, 0.15);
  border-radius: 20px;
  padding: 1.25rem;
}
.brief-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 0.4rem;
}
.brief-num {
  font-size: 1.05rem;
  font-weight: 700;
  color: #fff;
}
.brief-date {
  font-size: 0.78rem;
  color: rgba(255, 255, 255, 0.65);
  margin: 0 0 0.55rem;
}
.brief-title {
  font-size: 0.95rem;
  color: #fff;
  line-height: 1.4;
  margin: 0 0 0.25rem;
}
.brief-more {
  width: 100%;
  padding: 0.62rem;
  background: rgba(255, 255, 255, 0.15);
  border: 1px solid rgba(255, 255, 255, 0.25);
  color: #fff;
  border-radius: 12px;
  font-size: 0.9rem;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.15s;
}
.brief-more:hover {
  background: rgba(255, 255, 255, 0.25);
}

@media (min-width: 900px) {
  .brief-card {
    min-height: 280px;
  }
}
</style>
