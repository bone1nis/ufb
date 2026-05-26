<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import api from '@/shared/api/axios'
import { API_BASE_URL } from '@/shared/config/env'
import { statusLabel } from '@/entities/submission/model/types'
import type { Submission } from '@/entities/submission/model/types'
import { fmtDateTime } from '@/shared/lib/date'
import attachIcon from '@/assets/icons/attach.svg'
import chainIcon from '@/assets/icons/chain.svg'

const route        = useRoute()
const sub          = ref<Submission | null>(null)
const loading      = ref(true)
const error        = ref('')
const score        = ref<number | ''>('')
const comment      = ref('')
const grading      = ref(false)
const gradeError   = ref('')
const gradeSuccess = ref(false)

onMounted(async () => {
  try {
    const { data } = await api.get(`/api/teacher/submissions/${route.params.id}`)
    sub.value = data
    if (data.grade) { score.value = data.grade.score; comment.value = data.grade.comment ?? '' }
  } catch {
    error.value = 'Не удалось загрузить сдачу'
  } finally {
    loading.value = false
  }
})

async function saveGrade() {
  if (score.value === '') { gradeError.value = 'Укажите оценку'; return }
  const num = Number(score.value)
  if (isNaN(num) || num < 0 || num > 100) { gradeError.value = 'Оценка должна быть от 0 до 100'; return }
  score.value = num
  gradeError.value = ''; grading.value = true; gradeSuccess.value = false
  try {
    await api.post(`/api/teacher/submissions/${route.params.id}/grade`, {
      score: score.value, comment: comment.value,
    })
    if (sub.value) sub.value.status = 'graded'
    gradeSuccess.value = true
  } catch (e: any) {
    gradeError.value = e?.response?.data?.message ?? 'Ошибка'
  } finally {
    grading.value = false
  }
}

function downloadUrl(itemId: number) {
  return `${API_BASE_URL}/api/teacher/submissions/${route.params.id}/files/${itemId}`
}
</script>

<template>
  <div class="page-wrap">

    <RouterLink to="/teacher/submissions" class="back-link">← Назад</RouterLink>

    <div v-if="loading" class="text-muted mt-3" style="padding:2rem;text-align:center">Загрузка...</div>
    <div v-else-if="error" class="alert alert-error mt-3">{{ error }}</div>

    <template v-else-if="sub">

      <!-- Info card -->
      <div class="glass-card mt-2">
        <div class="info-header">
          <div>
            <div class="detail-hw-title">{{ sub.homework?.title }}</div>
            <p class="meta">{{ (sub as any).student?.name }} · {{ (sub as any).student?.email }}</p>
            <p class="meta">Сдано: {{ fmtDateTime(sub.submitted_at) }}</p>
          </div>
          <span class="badge" :class="sub.status === 'graded' ? 'badge-graded' : 'badge-pending'">
            {{ statusLabel[sub.status] }}
          </span>
        </div>
      </div>

      <!-- Materials -->
      <div class="glass-card mt-2">
        <h2>Материалы</h2>
        <div v-for="item in sub.items ?? []" :key="item.id" class="material-row">
          <template v-if="item.type === 'link'">
            <div class="mat-ico-wrap" aria-hidden="true">
              <img :src="chainIcon" alt="" width="18" height="18">
            </div>
            <div class="mat-info">
              <a :href="item.url!" target="_blank" rel="noopener" class="mat-link">{{ item.url }}</a>
            </div>
          </template>
          <template v-else>
            <div class="mat-ico-wrap" aria-hidden="true">
              <img :src="attachIcon" alt="" width="18" height="18">
            </div>
            <div class="mat-info">
              <div class="mat-name">{{ item.original_name }}</div>
              <div class="mat-size">{{ item.file_size ? (item.file_size / 1024).toFixed(1) + ' KB' : '' }}</div>
            </div>
            <a :href="downloadUrl(item.id)" class="btn btn-outline" style="font-size:.8rem;padding:.3rem .8rem;flex-shrink:0" download>
              Скачать
            </a>
          </template>
        </div>
        <div v-if="!sub.items?.length" class="text-muted" style="padding:.75rem 0">Нет вложений</div>
      </div>

      <!-- Grade form -->
      <div class="glass-card mt-2">
        <h2>{{ sub.status === 'graded' ? 'Изменить оценку' : 'Выставить оценку' }}</h2>

        <div v-if="sub.status === 'graded'" class="current-grade">
          <span class="score-big">{{ score }}</span>
          <span class="score-max">/ 100</span>
        </div>

        <div class="grade-form mt-2">
          <div class="field">
            <label>Балл (0–100)</label>
            <input
              v-model.number="score"
              type="number" min="0" max="100"
              class="input"
              placeholder="0–100"
            />
          </div>
          <div class="field mt-2">
            <label>Комментарий</label>
            <textarea v-model="comment" class="textarea" placeholder="Отлично, но..."></textarea>
          </div>
          <div v-if="gradeError" class="alert alert-error mt-2">{{ gradeError }}</div>
          <div v-if="gradeSuccess" class="alert alert-info mt-2">Оценка сохранена</div>
          <button class="btn btn-primary w-full mt-2" style="padding:.9rem;border-radius:12px" :disabled="grading" @click="saveGrade">
            {{ grading ? 'Сохраняем...' : (sub.status === 'graded' ? 'Обновить оценку' : 'Выставить оценку') }}
          </button>
        </div>
      </div>

    </template>
  </div>
</template>

<style scoped>
.page-wrap { padding-bottom: 2rem; }

@media (max-width: 640px) {
  .page-wrap {
    padding-top: calc(var(--layout-header-offset-mobile) + 0.35rem);
  }
}

.back-link {
  display: inline-block; font-size: .875rem;
  color: rgba(255,255,255,0.65); margin-top: .5rem; text-decoration: none;
}
.back-link:hover { color: #fff; }

.glass-card {
  background: rgba(255,255,255,0.11);
  backdrop-filter: blur(14px);
  border: 1px solid rgba(255,255,255,0.14);
  border-radius: 18px;
  padding: 1.25rem;
}

.info-header { display: flex; align-items: flex-start; justify-content: space-between; gap: 1rem; }
.detail-hw-title {
  font-size: 1.25rem;
  font-weight: 700;
  color: #fff;
  line-height: 1.3;
  margin: 0;
}
.meta { font-size: .83rem; color: rgba(255,255,255,0.6); margin-top: .25rem; }

.material-row {
  display: flex; align-items: flex-start; gap: .75rem;
  padding: .75rem 0;
  border-bottom: 1px solid rgba(255,255,255,0.08);
}
.material-row:last-child { border-bottom: none; }
.mat-ico-wrap {
  flex-shrink: 0;
  width: 40px;
  height: 40px;
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.95);
  display: flex;
  align-items: center;
  justify-content: center;
}
.mat-link  { color: #a78bfa; word-break: break-all; font-size: .9rem; }
.mat-link:hover { color: #c4b5fd; }
.mat-info  { flex: 1; min-width: 0; }
.mat-name  { font-weight: 500; color: #fff; word-break: break-all; font-size: .9rem; }
.mat-size  { font-size: .78rem; color: rgba(255,255,255,0.5); margin-top: 2px; }

.current-grade { display: flex; align-items: baseline; gap: .35rem; margin-top: .5rem; }
.score-big { font-size: 2.5rem; font-weight: 800; color: #fff; line-height: 1; }
.score-max { font-size: 1rem; color: rgba(255,255,255,0.55); }

.grade-form { max-width: 520px; }

@media (min-width: 641px) {
  .grade-form { max-width: 100%; }
}
</style>
