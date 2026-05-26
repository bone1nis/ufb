<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue'
import { useRoute } from 'vue-router'
import api from '@/shared/api/axios'
import { statusLabel } from '@/entities/submission/model/types'
import type { Submission } from '@/entities/submission/model/types'
import { fmtDateTime, fmtDate } from '@/shared/lib/date'
import timeIcon from '@/assets/icons/time.svg'
import accessIcon from '@/assets/icons/access.svg'
import attachIcon from '@/assets/icons/attach.svg'
import chainIcon from '@/assets/icons/chain.svg'

const route = useRoute()
const sub = ref<Submission | null>(null)
const loading = ref(true)
const error = ref('')

const scoreCap = computed(() => (sub.value?.grade ? Math.min(sub.value.grade.score, 100) : 0))

const hwTitle = computed(
  () => sub.value?.homework?.title ?? `Дз №${sub.value?.homework_id ?? ''}`,
)

async function load() {
  loading.value = true
  error.value = ''
  sub.value = null
  try {
    const { data } = await api.get<Submission>(`/api/student/submissions/${route.params.id}`)
    sub.value = data
  }
  catch {
    error.value = 'Не удалось загрузить сдачу'
  }
  finally {
    loading.value = false
  }
}

onMounted(load)
watch(() => route.params.id, load)
</script>

<template>
  <div class="detail-page">
    <RouterLink to="/student/submissions" class="back-link">← Назад</RouterLink>

    <div v-if="loading" class="state">Загрузка...</div>
    <div v-else-if="error" class="state state--err">{{ error }}</div>

    <template v-else-if="sub">
      <article class="hero-card">
        <div class="hero-top">
          <div class="hero-titles">
            <span class="dz-num">{{ hwTitle }}</span>
            <p class="submitted-line">Сдано: {{ fmtDate(sub.submitted_at) }}</p>
          </div>
          <span
            v-if="sub.status === 'graded'"
            class="pill pill--ok"
          >
            <span>{{ statusLabel[sub.status] }}</span>
            <img :src="accessIcon" alt="" width="15" height="15" class="pill-ico">
          </span>
          <span
            v-else
            class="pill pill--wait"
          >
            <span>{{ statusLabel[sub.status] }}</span>
            <img :src="timeIcon" alt="" width="15" height="15" class="pill-ico">
          </span>
        </div>

        <p v-if="sub.homework?.description" class="hw-desc">{{ sub.homework.description }}</p>
        <p v-else class="hw-desc hw-desc--muted">Описание задания не указано.</p>

        <template v-if="sub.status === 'graded' && sub.grade">
          <div class="grade-block">
            <p class="sec-label">Оценка</p>
            <div class="score-row">
              <span class="score-big">{{ scoreCap }}</span>
              <span class="score-max">/100</span>
            </div>
            <div class="score-bar-wrap">
              <div class="score-bar" :style="{ width: scoreCap + '%' }" />
            </div>
            <p v-if="sub.grade.comment" class="teacher-comment">
              <span class="comment-label">Комментарий преподавателя</span><br>
              {{ sub.grade.comment }}
            </p>
            <p class="graded-meta">Проверено: {{ fmtDateTime(sub.grade.graded_at) }}</p>
          </div>
        </template>

        <div class="mat-section">
          <p class="sec-label">Материалы</p>
          <div v-for="item in sub.items ?? []" :key="item.id" class="mat-row">
            <template v-if="item.type === 'link'">
              <div class="mat-ico-wrap" aria-hidden="true">
                <img :src="chainIcon" alt="" width="18" height="18">
              </div>
              <a :href="item.url!" target="_blank" rel="noopener" class="mat-link">{{ item.url }}</a>
            </template>
            <template v-else>
              <div class="mat-ico-wrap" aria-hidden="true">
                <img :src="attachIcon" alt="" width="18" height="18">
              </div>
              <div class="mat-info">
                <div class="mat-name">{{ item.original_name }}</div>
                <div class="mat-size">{{ item.file_size ? (item.file_size / 1024).toFixed(1) + ' KB' : '' }}</div>
              </div>
            </template>
          </div>
          <div v-if="!sub.items?.length" class="mat-empty">Нет вложений</div>
        </div>

        <RouterLink
          v-if="sub.status === 'pending'"
          :to="`/student/submissions/${sub.id}/edit`"
          class="btn-edit"
        >
          Редактировать
        </RouterLink>
      </article>
    </template>
  </div>
</template>

<style scoped>
.detail-page {
  width: 100vw;
  margin-left: calc(50% - 50vw);
  margin-right: calc(50% - 50vw);
  min-height: calc(100svh - 58px);
  padding: 0 1rem 2.5rem;
  box-sizing: border-box;
  background-color: #5b21b6;
  background-image: url('@/assets/bg/gradient-2.png');
  background-repeat: repeat-x;
  background-size: auto 100%;
  background-position: top center;
}

.back-link {
  display: inline-block;
  font-size: 0.875rem;
  color: rgba(255, 255, 255, 0.65);
  margin-top: 0.65rem;
  margin-bottom: 1rem;
  text-decoration: none;
}
.back-link:hover {
  color: #fff;
}

.state {
  text-align: center;
  color: rgba(255, 255, 255, 0.65);
  padding: 2rem;
}
.state--err {
  color: #fecaca;
}

.hero-card {
  max-width: 420px;
  margin: 0 auto;
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(18px);
  -webkit-backdrop-filter: blur(18px);
  border: 1px solid rgba(255, 255, 255, 0.22);
  border-radius: 24px;
  padding: 1.25rem 1.2rem 1.5rem;
  box-shadow: 0 12px 48px rgba(0, 0, 0, 0.2);
}

.hero-top {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 0.75rem;
  margin-bottom: 0.75rem;
}
.hero-titles {
  min-width: 0;
}
.dz-num {
  display: block;
  font-size: 1.15rem;
  font-weight: 700;
  color: #fff;
}
.submitted-line {
  margin: 0.35rem 0 0;
  font-size: 0.8rem;
  color: rgba(255, 255, 255, 0.7);
}

.pill {
  flex-shrink: 0;
  display: inline-flex;
  align-items: center;
  gap: 0.28rem;
  font-size: 0.72rem;
  font-weight: 600;
  padding: 0.32rem 0.55rem 0.32rem 0.65rem;
  border-radius: 999px;
  color: #fff;
}
.pill--ok {
  background: #16a34a;
  box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.2);
}
.pill--wait {
  background: #ca8a04;
  box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.2);
}
.pill-ico {
  display: block;
}

.hw-desc {
  font-size: 0.88rem;
  line-height: 1.45;
  color: rgba(255, 255, 255, 0.92);
  margin: 0 0 1rem;
}
.hw-desc--muted {
  color: rgba(255, 255, 255, 0.5);
}

.sec-label {
  font-size: 0.82rem;
  font-weight: 600;
  color: #fff;
  margin: 0 0 0.4rem;
}

.grade-block {
  margin-bottom: 1rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.12);
}
.score-row {
  display: flex;
  align-items: baseline;
  gap: 0.25rem;
  margin-bottom: 0.45rem;
}
.score-big {
  font-size: 2rem;
  font-weight: 800;
  color: #fff;
  line-height: 1;
}
.score-max {
  font-size: 0.95rem;
  color: rgba(255, 255, 255, 0.55);
}
.score-bar-wrap {
  height: 6px;
  background: rgba(255, 255, 255, 0.18);
  border-radius: 4px;
  overflow: hidden;
  margin-bottom: 0.75rem;
}
.score-bar {
  height: 100%;
  background: #fff;
  border-radius: 4px;
}
.teacher-comment {
  font-size: 0.88rem;
  color: rgba(255, 255, 255, 0.9);
  line-height: 1.45;
  margin: 0 0 0.5rem;
}
.comment-label {
  font-weight: 600;
  color: rgba(255, 255, 255, 0.95);
}
.graded-meta {
  font-size: 0.78rem;
  color: rgba(255, 255, 255, 0.55);
  margin: 0;
}

.mat-section {
  margin-top: 0.25rem;
}
.mat-row {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  padding: 0.65rem 0;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}
.mat-row:last-of-type {
  border-bottom: none;
}
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
.mat-info {
  flex: 1;
  min-width: 0;
}
.mat-name {
  font-weight: 500;
  color: #fff;
  font-size: 0.9rem;
  word-break: break-all;
}
.mat-size {
  font-size: 0.76rem;
  color: rgba(255, 255, 255, 0.5);
  margin-top: 2px;
}
.mat-link {
  flex: 1;
  min-width: 0;
  color: #c4b5fd;
  font-size: 0.88rem;
  word-break: break-all;
  padding-top: 0.35rem;
}
.mat-link:hover {
  color: #fff;
}
.mat-empty {
  font-size: 0.85rem;
  color: rgba(255, 255, 255, 0.45);
  padding: 0.5rem 0;
}

.btn-edit {
  display: block;
  width: 100%;
  margin-top: 1.25rem;
  text-align: center;
  padding: 0.72rem 1rem;
  border-radius: 999px;
  border: none;
  background: #7c25ff;
  color: #fff;
  font-size: 1rem;
  font-weight: 500;
  text-decoration: none;
  box-shadow: 0 8px 24px rgba(124, 37, 255, 0.45);
  transition: opacity 0.15s, transform 0.12s;
}
.btn-edit:hover {
  opacity: 0.92;
  text-decoration: none;
  transform: translateY(-1px);
}

@media (min-width: 640px) {
  .hero-card {
    max-width: 480px;
    padding: 1.5rem 1.5rem 1.75rem;
  }
}

@media (max-width: 640px) {
  .detail-page {
    min-height: 100svh;
    padding: calc(var(--layout-header-offset-mobile) + 0.65rem) 1rem 2.5rem;
  }
}
</style>
