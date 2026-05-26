<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import type { Homework } from '@/entities/homework/model/types'
import { fmtDeadline } from '@/shared/lib/date'
import api from '@/shared/api/axios'

const list = ref<Homework[]>([])
const loading = ref(true)
const sortDesc = ref(true)

const sorted = computed(() => {
  const arr = [...list.value]
  arr.sort((a, b) => {
    const ta = a.deadline ? new Date(a.deadline).getTime() : Number.POSITIVE_INFINITY
    const tb = b.deadline ? new Date(b.deadline).getTime() : Number.POSITIVE_INFINITY
    const diff = ta - tb
    return sortDesc.value ? diff : -diff
  })
  return arr
})

onMounted(async () => {
  try {
    const { data } = await api.get<Homework[]>('/api/student/homeworks/todo')
    list.value = data
  } finally {
    loading.value = false
  }
})

function submitUrl(hwId: number) {
  return `/student/submit?homework_id=${hwId}`
}
</script>

<template>
  <div class="due-page">
    <div class="due-inner">
      <div class="due-head">
        <h1 class="due-title">Несданные домашки</h1>
        <button type="button" class="sort-pill" @click="sortDesc = !sortDesc">
          {{ sortDesc ? 'Сначала ближайший дедлайн' : 'Сначала без срока / позже' }}
          <span class="chev">▾</span>
        </button>
      </div>

      <div v-if="loading" class="state">Загрузка…</div>
      <div v-else-if="!sorted.length" class="state">Все задания сданы или пока нет новых.</div>

      <div v-else class="due-list">
        <article v-for="hw in sorted" :key="hw.id" class="due-card">
          <div class="due-card-top">
            <span class="due-hw-id">ДЗ №{{ hw.id }}</span>
            <span v-if="hw.deadline" class="due-deadline">До {{ fmtDeadline(hw.deadline) }}</span>
            <span v-else class="due-deadline due-deadline--soft">Без срока</span>
          </div>
          <p class="due-hw-title">{{ hw.title }}</p>
          <p v-if="hw.description" class="due-desc">{{ hw.description }}</p>
          <p class="due-meta">{{ hw.project }} · {{ hw.direction }} · {{ hw.course }} курс</p>

          <div class="due-mat">
            <p class="due-mat-label">Материалы</p>
            <p class="due-mat-empty">Материалы задания смотри в описании или у методиста в курсе.</p>
          </div>

          <RouterLink :to="submitUrl(hw.id)" class="due-submit">
            <span>Сдать</span>
            <svg class="due-submit-ico" width="20" height="20" viewBox="0 0 24 24" fill="none" aria-hidden="true">
              <path d="M12 4v12m0 0l4-4m-4 4l-4-4M6 20h12" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" />
            </svg>
          </RouterLink>
        </article>
      </div>
    </div>
  </div>
</template>

<style scoped>
.due-page {
  width: 100vw;
  margin-left: calc(50% - 50vw);
  margin-right: calc(50% - 50vw);
  min-height: calc(100svh - 58px);
  padding-bottom: 2rem;
  background-color: #5b21b6;
  background-image: url('@/assets/bg/gradient-2.png');
  background-repeat: repeat-x;
  background-size: auto 100%;
  background-position: top center;
}

.due-inner {
  max-width: 420px;
  margin: 0 auto;
  padding: 1.15rem 1rem 0;
}

.due-head {
  margin-bottom: 1rem;
}
.due-title {
  font-size: 1.25rem;
  font-weight: 700;
  color: #fff;
  margin: 0 0 0.5rem;
}
.sort-pill {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  border: none;
  background: none;
  color: rgba(255, 255, 255, 0.85);
  font-size: 0.88rem;
  font-weight: 500;
  cursor: pointer;
  padding: 0;
}
.chev {
  font-size: 0.7rem;
  opacity: 0.75;
}

.state {
  text-align: center;
  color: rgba(255, 255, 255, 0.6);
  padding: 2rem 0;
}

.due-list {
  display: flex;
  flex-direction: column;
  gap: 0.85rem;
}

.due-card {
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(14px);
  border: 1px solid rgba(255, 255, 255, 0.22);
  border-radius: 22px;
  padding: 1.15rem 1.2rem 1.25rem;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.15);
}

.due-card-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  margin-bottom: 0.45rem;
}
.due-hw-id {
  font-weight: 700;
  font-size: 1.05rem;
  color: #fff;
}
.due-deadline {
  flex-shrink: 0;
  font-size: 0.72rem;
  font-weight: 600;
  color: #fff;
  background: rgba(124, 37, 255, 0.55);
  border: 1px solid rgba(255, 255, 255, 0.35);
  border-radius: 999px;
  padding: 0.2rem 0.55rem;
}
.due-deadline--soft {
  background: rgba(255, 255, 255, 0.12);
  color: rgba(255, 255, 255, 0.85);
}

.due-hw-title {
  font-size: 0.95rem;
  font-weight: 600;
  color: #fff;
  line-height: 1.35;
  margin: 0 0 0.4rem;
}
.due-desc {
  font-size: 0.84rem;
  color: rgba(255, 255, 255, 0.82);
  line-height: 1.45;
  margin: 0 0 0.5rem;
}
.due-meta {
  font-size: 0.78rem;
  color: rgba(255, 255, 255, 0.55);
  margin: 0 0 0.85rem;
}

.due-mat {
  border-top: 1px solid rgba(255, 255, 255, 0.12);
  padding-top: 0.75rem;
  margin-bottom: 1rem;
}
.due-mat-label {
  font-size: 0.82rem;
  font-weight: 600;
  color: #fff;
  margin: 0 0 0.35rem;
}
.due-mat-empty {
  display: flex;
  align-items: flex-start;
  gap: 0.5rem;
  font-size: 0.78rem;
  color: rgba(255, 255, 255, 0.55);
  margin: 0;
}
.due-mat-empty::before {
  content: '';
  display: block;
  width: 36px;
  height: 36px;
  flex-shrink: 0;
  border-radius: 10px;
  background: rgba(255, 255, 255, 0.92) url('@/assets/icons/attach.svg') center / 18px no-repeat;
}

.due-submit {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  width: 100%;
  padding: 0.7rem 1rem;
  border-radius: 999px;
  border: 1px solid rgba(255, 255, 255, 0.45);
  background: rgba(255, 255, 255, 0.12);
  color: #fff;
  font-size: 1rem;
  font-weight: 500;
  text-decoration: none;
  transition: background 0.15s, border-color 0.15s;
}
.due-submit:hover {
  background: rgba(255, 255, 255, 0.2);
  text-decoration: none;
}
.due-submit-ico {
  opacity: 0.95;
}

@media (min-width: 900px) {
  .due-inner {
    max-width: 480px;
  }
}

@media (max-width: 640px) {
  .due-page {
    min-height: 100svh;
  }
  .due-inner {
    padding-top: calc(var(--layout-header-offset-mobile) + 1.15rem);
  }
}
</style>
