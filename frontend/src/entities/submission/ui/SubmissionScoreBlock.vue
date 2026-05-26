<script setup lang="ts">
import { computed } from 'vue'
import type { Submission } from '@/entities/submission/model/types'
import { submissionScore } from '@/entities/submission/lib/submissionScore'

const props = defineProps<{
  submission: Submission
}>()

const score = computed(() => submissionScore(props.submission))
</script>

<template>
  <div v-if="score != null" class="score-block">
    <p class="score-label">Оценка</p>
    <div class="score-row">
      <span class="score-big">{{ score }}</span>
      <span class="score-max">/100</span>
    </div>
    <div class="score-track">
      <div class="score-fill" :style="{ width: score + '%' }" />
    </div>
  </div>
</template>

<style scoped>
.score-block {
  margin-bottom: 1rem;
}
.score-label {
  margin: 0 0 0.35rem;
  font-size: 0.78rem;
  font-weight: 500;
  color: rgba(255, 255, 255, 0.72);
}
.score-row {
  display: flex;
  align-items: baseline;
  gap: 0.2rem;
  margin-bottom: 0.45rem;
}
.score-big {
  font-size: 1.75rem;
  font-weight: 800;
  color: #fff;
  line-height: 1;
}
.score-max {
  font-size: 0.95rem;
  font-weight: 600;
  color: rgba(255, 255, 255, 0.65);
}
.score-track {
  height: 6px;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 999px;
  overflow: hidden;
}
.score-fill {
  height: 100%;
  background: #fff;
  border-radius: 999px;
  transition: width 0.5s ease;
}
</style>
