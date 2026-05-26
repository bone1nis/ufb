<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import api from '@/shared/api/axios'
import { statusLabel } from '@/entities/submission/model/types'
import type { Submission } from '@/entities/submission/model/types'
import { fmtDate } from '@/shared/lib/date'
import checkIcon from '@/assets/icons/check.svg'
import timeIcon from '@/assets/icons/time.svg'
import accessIcon from '@/assets/icons/access.svg'
import userIcon from '@/assets/icons/user.svg'
import SubmissionListSearchBar from '@/features/submission-filter/ui/SubmissionListSearchBar.vue'
import SubmissionFilterSheet from '@/features/submission-filter/ui/SubmissionFilterSheet.vue'
import { homeworkGroupTag } from '@/shared/lib/homeworkGroupTag'

const router = useRouter()
const all = ref<Submission[]>([])
const loading = ref(true)

/** Все | Не проверено | Проверено */
const segmentStatus = ref<'' | 'pending' | 'graded'>('')
const searchQuery = ref('')
const sortDir = ref<'desc' | 'asc'>('desc')
const showSort = ref(false)
const showFilter = ref(false)

const tempSort = ref<'desc' | 'asc'>('desc')

const appliedDirections = ref<string[]>([])
const appliedCourses = ref<number[]>([])

const tempDirections = ref<string[]>([])
const tempCourses = ref<number[]>([])

onMounted(async () => {
  try {
    const { data } = await api.get('/api/student/submissions')
    all.value = data
  } finally {
    loading.value = false
  }
})

function setDir(v: string, on: boolean) {
  const arr = tempDirections.value
  const i = arr.indexOf(v)
  if (on && i < 0) arr.push(v)
  if (!on && i >= 0) arr.splice(i, 1)
}
function setCourse(v: number, on: boolean) {
  const arr = tempCourses.value
  const i = arr.indexOf(v)
  if (on && i < 0) arr.push(v)
  if (!on && i >= 0) arr.splice(i, 1)
}

const filtered = computed(() => {
  let list = all.value.filter(s => s.status === 'pending' || s.status === 'graded')

  if (segmentStatus.value)
    list = list.filter(s => s.status === segmentStatus.value)

  const q = searchQuery.value.trim().toLowerCase()
  if (q) {
    list = list.filter(s => {
      const title = (s.homework?.title ?? '').toLowerCase()
      const proj = (s.homework?.project ?? '').toLowerCase()
      const dir = (s.homework?.direction ?? '').toLowerCase()
      const tag = homeworkGroupTag(s.homework).toLowerCase()
      return title.includes(q) || proj.includes(q) || dir.includes(q) || tag.includes(q) || String(s.id).includes(q)
    })
  }

  if (appliedDirections.value.length)
    list = list.filter(s => appliedDirections.value.includes(s.homework?.direction ?? ''))
  if (appliedCourses.value.length)
    list = list.filter(s => s.homework != null && appliedCourses.value.includes(s.homework.course))

  return [...list].sort((a, b) => {
    const diff = new Date(a.submitted_at).getTime() - new Date(b.submitted_at).getTime()
    return sortDir.value === 'desc' ? -diff : diff
  })
})

function openSort() {
  tempSort.value = sortDir.value
  showSort.value = true
}
function applySort() {
  sortDir.value = tempSort.value
  showSort.value = false
}

function openFilter() {
  tempDirections.value = [...appliedDirections.value]
  tempCourses.value = [...appliedCourses.value]
  showFilter.value = true
}
function applyFilter() {
  appliedDirections.value = [...tempDirections.value]
  appliedCourses.value = [...tempCourses.value]
}

const sortTriggerLabel = computed(() =>
  sortDir.value === 'desc' ? 'Сначала недавние' : 'Сначала старые',
)
</script>

<template>
  <div class="list-bleed">
    <div class="list-inner">

      <!-- Сегменты статуса -->
      <div class="segment" role="tablist">
        <button
          type="button"
          class="segment-btn"
          :class="{ active: segmentStatus === '' }"
          @click="segmentStatus = ''"
        >Все</button>
        <button
          type="button"
          class="segment-btn"
          :class="{ active: segmentStatus === 'pending' }"
          @click="segmentStatus = 'pending'"
        >Не проверено</button>
        <button
          type="button"
          class="segment-btn"
          :class="{ active: segmentStatus === 'graded' }"
          @click="segmentStatus = 'graded'"
        >Проверено</button>
      </div>

      <!-- Поиск + фильтр -->
      <SubmissionListSearchBar v-model="searchQuery" @filter-click="openFilter" />

      <SubmissionFilterSheet
        v-model="showFilter"
        :temp-directions="tempDirections"
        :temp-courses="tempCourses"
        @apply="applyFilter"
        @toggle-dir="setDir"
        @toggle-course="setCourse"
      />

      <!-- Сортировка -->
      <button type="button" class="sort-trigger" @click="openSort">
        {{ sortTriggerLabel }}
        <span class="chev">▾</span>
      </button>

      <div v-if="loading" class="state-msg">Загрузка...</div>
      <div v-else-if="!filtered.length" class="state-msg">Ничего не найдено</div>

      <div v-else class="sub-list">
        <div
          v-for="s in filtered"
          :key="s.id"
          class="sub-card"
          @click="router.push(`/student/submissions/${s.id}`)"
        >
          <div class="sub-avatar" aria-hidden="true">
            <img :src="userIcon" alt="" class="sub-avatar-img" width="16" height="16">
          </div>
          <div class="sub-body">
            <div class="sub-row-top">
              <span class="sub-title">{{ s.homework?.title ?? '—' }}</span>
              <span
                class="pill-status"
                :class="s.status === 'graded' ? 'pill-status--ok' : 'pill-status--wait'"
              >
                <span class="pill-status__text">{{ statusLabel[s.status] }}</span>
                <img
                  :src="s.status === 'graded' ? accessIcon : timeIcon"
                  alt=""
                  width="15"
                  height="15"
                  class="pill-status__ico"
                >
              </span>
            </div>
            <div class="sub-meta">{{ s.homework?.project }} · {{ s.homework?.direction }}</div>
            <div class="sub-row-bottom">
              <span class="sub-date">{{ fmtDate(s.submitted_at) }}</span>
              <span v-if="s.grade" class="sub-score">{{ Math.min(s.grade.score, 100) }}<span class="sub-max">/100</span></span>
            </div>
            <div v-if="s.grade" class="score-track">
              <div class="score-fill" :style="{ width: Math.min(s.grade.score, 100) + '%' }" />
            </div>
          </div>
        </div>
      </div>

      <!-- Сортировка -->
      <Teleport to="body">
        <div v-if="showSort" class="popup-overlay" @click.self="showSort = false">
          <div class="popup-sheet">
            <div class="popup-handle" />
            <h2 class="popup-title">Сортировка</h2>
            <div class="popup-options">
              <label class="popup-option" :class="{ active: tempSort === 'desc' }">
                <input v-model="tempSort" type="radio" value="desc" class="sr-only">
                <span>Сначала недавние</span>
                <span v-if="tempSort === 'desc'" class="popup-check">
                  <img :src="checkIcon" alt="" width="14" height="14">
                </span>
              </label>
              <label class="popup-option" :class="{ active: tempSort === 'asc' }">
                <input v-model="tempSort" type="radio" value="asc" class="sr-only">
                <span>Сначала старые</span>
                <span v-if="tempSort === 'asc'" class="popup-check">
                  <img :src="checkIcon" alt="" width="14" height="14">
                </span>
              </label>
            </div>
            <button type="button" class="btn-apply" @click="applySort">Применить</button>
          </div>
        </div>
      </Teleport>

    </div>
  </div>
</template>

<style scoped>
.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}

.list-bleed {
  width: 100vw;
  margin-left: calc(50% - 50vw);
  margin-right: calc(50% - 50vw);
  background: linear-gradient(180deg, #5b21b6 0%, #6d28d9 42%, #7c3aed 100%);
  min-height: calc(100svh - 58px);
  padding-bottom: 1.5rem;
}
.list-inner {
  max-width: 860px;
  margin: 0 auto;
  padding: 1.15rem 1rem 0;
}

.segment {
  display: flex;
  background: rgba(0, 0, 0, 0.22);
  border-radius: 999px;
  padding: 4px;
  gap: 2px;
  margin-bottom: 1rem;
}
.segment-btn {
  flex: 1;
  border: none;
  background: transparent;
  color: rgba(255, 255, 255, 0.78);
  font-size: 0.82rem;
  font-weight: 500;
  padding: 0.55rem 0.4rem;
  border-radius: 999px;
  cursor: pointer;
  transition: background 0.15s, color 0.15s, box-shadow 0.15s;
}
.segment-btn.active {
  background: rgba(124, 37, 255, 0.55);
  color: #fff;
  font-weight: 600;
  box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.35);
}

.sort-trigger {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  border: none;
  background: none;
  font-size: 0.88rem;
  font-weight: 500;
  color: rgba(255, 255, 255, 0.88);
  cursor: pointer;
  padding: 0.35rem 0;
  margin-bottom: 1rem;
}
.chev {
  font-size: 0.7rem;
  opacity: 0.75;
}

.state-msg {
  text-align: center;
  padding: 2.5rem 1rem;
  color: rgba(255, 255, 255, 0.55);
  font-size: 0.95rem;
}

/* Карточки — «пилюли» на фиолетовом фоне */
.sub-list {
  display: flex;
  flex-direction: column;
  gap: 0.65rem;
}
.sub-card {
  display: flex;
  gap: 0.85rem;
  align-items: flex-start;
  padding: 1rem 1.15rem;
  background: rgba(255, 255, 255, 0.08);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.38);
  border-radius: 26px;
  cursor: pointer;
  box-shadow: 0 6px 24px rgba(0, 0, 0, 0.12);
  transition: transform 0.12s, background 0.15s, box-shadow 0.12s;
}
.sub-card:hover {
  transform: translateY(-1px);
  background: rgba(255, 255, 255, 0.12);
  box-shadow: 0 10px 32px rgba(0, 0, 0, 0.16);
}
.sub-avatar {
  flex-shrink: 0;
  box-sizing: border-box;
  width: 30px;
  height: 30px;
  padding: 7px;
  border-radius: 37px;
  background: rgba(255, 255, 255, 0.95);
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px solid rgba(255, 255, 255, 0.5);
}
.sub-avatar-img {
  display: block;
  width: 100%;
  height: 100%;
  object-fit: contain;
}
.sub-body {
  flex: 1;
  min-width: 0;
}
.sub-row-top {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 0.5rem;
  margin-bottom: 0.2rem;
}
.sub-title {
  font-weight: 600;
  font-size: 0.98rem;
  color: #fff;
  line-height: 1.3;
}
.pill-status {
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
.pill-status__text {
  line-height: 1.2;
}
.pill-status__ico {
  display: block;
  flex-shrink: 0;
}
.pill-status--ok {
  background: #16a34a;
  box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.2);
}
.pill-status--wait {
  background: #ca8a04;
  box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.2);
}
.sub-meta {
  font-size: 0.78rem;
  color: rgba(255, 255, 255, 0.62);
  margin-bottom: 0.35rem;
}
.sub-row-bottom {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
}
.sub-date {
  font-size: 0.8rem;
  color: rgba(255, 255, 255, 0.5);
}
.sub-score {
  font-weight: 700;
  font-size: 0.95rem;
  color: #fff;
}
.sub-max {
  font-weight: 500;
  font-size: 0.75rem;
  color: rgba(255, 255, 255, 0.55);
}
.score-track {
  margin-top: 0.45rem;
  height: 4px;
  background: rgba(255, 255, 255, 0.22);
  border-radius: 2px;
  overflow: hidden;
}
.score-fill {
  height: 100%;
  background: #fff;
  border-radius: 2px;
}

.popup-overlay {
  position: fixed;
  inset: 0;
  z-index: 100;
  background: rgba(15, 10, 30, 0.45);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: flex-end;
  justify-content: center;
}
.popup-sheet {
  width: 100%;
  max-width: 480px;
  background: #fff;
  color: #1a1a1a;
  border-radius: 20px 20px 0 0;
  padding: 0.5rem 1.25rem 1.75rem;
  box-shadow: 0 -12px 48px rgba(0, 0, 0, 0.18);
  border: 1px solid rgba(0, 0, 0, 0.06);
  border-bottom: none;
}
.popup-handle {
  width: 36px;
  height: 4px;
  background: #e5e7eb;
  border-radius: 2px;
  margin: 0 auto 0.75rem;
}
.popup-title {
  font-size: 1.05rem;
  font-weight: 700;
  color: #111827;
  margin-bottom: 0.75rem;
}
.popup-options {
  display: flex;
  flex-direction: column;
  gap: 0.12rem;
}
.popup-option {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.8rem 0.85rem;
  border-radius: 12px;
  cursor: pointer;
  color: #374151;
  font-size: 0.95rem;
}
.popup-option.active {
  background: rgba(124, 37, 255, 0.08);
  font-weight: 600;
}
.popup-check {
  display: flex;
  align-items: center;
  line-height: 0;
}

.btn-apply {
  width: 100%;
  margin-top: 0.5rem;
  padding: 1rem;
  border: none;
  border-radius: 999px;
  background: #7c25ff;
  color: #fff;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
}
.btn-apply:hover {
  background: #6b18f0;
}

@media (min-width: 641px) {
  .popup-overlay {
    align-items: center;
  }
  .popup-sheet {
    max-width: 400px;
    border-radius: 16px;
    border: 1px solid #e5e7eb;
    padding: 1.25rem 1.35rem 1.5rem;
    box-shadow: 0 24px 64px rgba(0, 0, 0, 0.2);
  }
  .popup-handle {
    display: none;
  }
}

@media (max-width: 640px) {
  .list-bleed {
    min-height: 100svh;
  }
  .list-inner {
    padding-top: calc(var(--layout-header-offset-mobile) + 1.15rem);
  }
}
</style>
