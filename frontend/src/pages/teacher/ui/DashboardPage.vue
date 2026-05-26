<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/features/auth/model/useAuthStore'
import api from '@/shared/api/axios'
import VkLinkWidget from '@/features/vk-link/ui/VkLinkWidget.vue'
import SubmissionListSearchBar from '@/features/submission-filter/ui/SubmissionListSearchBar.vue'
import SubmissionFilterSheet from '@/features/submission-filter/ui/SubmissionFilterSheet.vue'
import HeroBand from '@/shared/ui/HeroBand.vue'
import ProfileHeader from '@/shared/ui/ProfileHeader.vue'
import type { Submission } from '@/entities/submission/model/types'
import { homeworkGroupTag } from '@/shared/lib/homeworkGroupTag'

const router = useRouter()
const authStore = useAuthStore()

const allSubs = ref<Submission[]>([])
const loading = ref(true)

const searchQuery = ref('')
const showFilter = ref(false)
const tempDirections = ref<string[]>([])
const tempProjects = ref<string[]>([])
const tempCourses = ref<number[]>([])
const appliedDirections = ref<string[]>([])
const appliedProjects = ref<string[]>([])
const appliedCourses = ref<number[]>([])

onMounted(async () => {
  try {
    const { data } = await api.get<Submission[]>('/api/teacher/submissions')
    allSubs.value = data
  }
  finally {
    loading.value = false
  }
})

const pendingCount = computed(() => allSubs.value.filter(s => s.status === 'pending').length)

function studentKey(s: Submission): string | null {
  if (s.student?.id != null)
    return `id:${s.student.id}`
  if (s.student?.email)
    return `em:${s.student.email}`
  if (s.student?.name)
    return `nm:${s.student.name}`
  return null
}

const studentRows = computed(() => {
  const m = new Map<string, Submission[]>()
  for (const s of allSubs.value) {
    const k = studentKey(s)
    if (k == null)
      continue
    if (!m.has(k))
      m.set(k, [])
    m.get(k)!.push(s)
  }
  const rows: { latest: Submission }[] = []
  for (const subs of m.values()) {
    const sorted = [...subs].sort(
      (a, b) => new Date(b.submitted_at).getTime() - new Date(a.submitted_at).getTime(),
    )
    if (sorted[0])
      rows.push({ latest: sorted[0] })
  }
  rows.sort(
    (a, b) => new Date(b.latest.submitted_at).getTime() - new Date(a.latest.submitted_at).getTime(),
  )
  return rows
})

function passesApplied(s: Submission): boolean {
  if (appliedDirections.value.length && !appliedDirections.value.includes(s.homework?.direction ?? ''))
    return false
  if (appliedProjects.value.length && !appliedProjects.value.includes(s.homework?.project ?? ''))
    return false
  if (appliedCourses.value.length && (!s.homework || !appliedCourses.value.includes(s.homework.course)))
    return false
  return true
}

const filteredRows = computed(() => {
  let list = studentRows.value.filter(({ latest }) => passesApplied(latest))
  const q = searchQuery.value.trim().toLowerCase()
  if (q) {
    list = list.filter(({ latest }) => {
      const name = (latest.student?.name ?? '').toLowerCase()
      const email = (latest.student?.email ?? '').toLowerCase()
      const title = (latest.homework?.title ?? '').toLowerCase()
      return name.includes(q) || email.includes(q) || title.includes(q)
    })
  }
  return list
})

function setDir(v: string, on: boolean) {
  const arr = tempDirections.value
  const i = arr.indexOf(v)
  if (on && i < 0)
    arr.push(v)
  if (!on && i >= 0)
    arr.splice(i, 1)
}
function setProj(v: string, on: boolean) {
  const arr = tempProjects.value
  const i = arr.indexOf(v)
  if (on && i < 0)
    arr.push(v)
  if (!on && i >= 0)
    arr.splice(i, 1)
}
function setCourse(v: number, on: boolean) {
  const arr = tempCourses.value
  const i = arr.indexOf(v)
  if (on && i < 0)
    arr.push(v)
  if (!on && i >= 0)
    arr.splice(i, 1)
}

function openFilter() {
  tempDirections.value = [...appliedDirections.value]
  tempProjects.value = [...appliedProjects.value]
  tempCourses.value = [...appliedCourses.value]
  showFilter.value = true
}

function applyFilter() {
  appliedDirections.value = [...tempDirections.value]
  appliedProjects.value = [...tempProjects.value]
  appliedCourses.value = [...tempCourses.value]
}

function rowClick(s: Submission) {
  router.push(`/teacher/submissions/${s.id}`)
}

function goReview() {
  router.push({ path: '/teacher/submissions', query: { status: 'pending' } })
}
</script>

<template>
  <div class="t-dash">
    <HeroBand variant="compact">
      <ProfileHeader
        :name="authStore.user?.name ?? ''"
        :tags="['Преподаватель']"
      />
      <VkLinkWidget class="vk-in-hero" />
    </HeroBand>

    <div class="t-dash-inner">
      <div class="t-pending-card">
        <p class="t-pending-label">Непроверенных работ</p>
        <p class="t-pending-num">{{ pendingCount }}</p>
      </div>

      <button type="button" class="t-btn-review" @click="goReview">
        Перейти к проверке
      </button>

      <SubmissionListSearchBar v-model="searchQuery" @filter-click="openFilter" />
      <SubmissionFilterSheet
        v-model="showFilter"
        :temp-directions="tempDirections"
        :temp-projects="tempProjects"
        :temp-courses="tempCourses"
        @apply="applyFilter"
        @toggle-dir="setDir"
        @toggle-proj="setProj"
        @toggle-course="setCourse"
      />

      <div v-if="loading" class="t-state">Загрузка…</div>
      <div v-else-if="!filteredRows.length" class="t-state">Нет учеников по фильтру.</div>
      <div v-else class="t-students">
        <button
          v-for="{ latest } in filteredRows"
          :key="studentKey(latest) ?? latest.id"
          type="button"
          class="t-student-row"
          @click="rowClick(latest)"
        >
          <span class="t-student-name">{{ latest.student?.name ?? '—' }}</span>
          <span class="t-student-tags">
            <span v-if="latest.homework" class="t-tag">{{ latest.homework.course }} курс</span>
            <span v-if="latest.homework" class="t-tag t-tag--accent">{{ homeworkGroupTag(latest.homework) }}</span>
          </span>
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.t-dash {
  min-height: calc(100dvh - 58px);
  padding: 0 0 2.5rem;
}

:deep(.vk-in-hero) {
  margin-bottom: 0;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.22);
  border-radius: 16px;
}

.t-dash-inner {
  max-width: 420px;
  margin: 0 auto;
  padding: 1.35rem 1rem 0;
}

.t-pending-card {
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border: 1px solid rgba(255, 255, 255, 0.22);
  border-radius: 22px;
  padding: 1rem 1.15rem 1.2rem;
  margin-bottom: 0.85rem;
  text-align: center;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.12);
}
.t-pending-label {
  margin: 0 0 0.35rem;
  font-size: 0.88rem;
  font-weight: 500;
  color: rgba(255, 255, 255, 0.88);
}
.t-pending-num {
  margin: 0;
  font-size: 2.5rem;
  font-weight: 800;
  color: #fff;
  line-height: 1;
}

.t-btn-review {
  display: block;
  width: 100%;
  margin-bottom: 1rem;
  padding: 0.72rem 1rem;
  border-radius: 999px;
  border: 1px solid rgba(255, 255, 255, 0.45);
  background: rgba(255, 255, 255, 0.12);
  color: #fff;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.15s, border-color 0.15s;
}
.t-btn-review:hover {
  background: rgba(255, 255, 255, 0.18);
}

.t-state {
  text-align: center;
  color: rgba(255, 255, 255, 0.6);
  padding: 1.5rem 0.5rem;
  font-size: 0.9rem;
}

.t-students {
  display: flex;
  flex-direction: column;
  gap: 0.65rem;
}
.t-student-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
  width: 100%;
  text-align: left;
  padding: 0.95rem 1.1rem;
  border-radius: 22px;
  border: 1px solid rgba(255, 255, 255, 0.28);
  background: rgba(255, 255, 255, 0.08);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  color: inherit;
  cursor: pointer;
  box-shadow: 0 6px 24px rgba(0, 0, 0, 0.1);
  transition: background 0.15s, transform 0.12s;
}
.t-student-row:hover {
  background: rgba(255, 255, 255, 0.12);
}
.t-student-name {
  font-size: 0.95rem;
  font-weight: 700;
  color: #fff;
  min-width: 0;
  flex: 1;
}
.t-student-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 0.35rem;
  justify-content: flex-end;
  flex-shrink: 0;
}
.t-tag {
  font-size: 0.68rem;
  font-weight: 600;
  color: #fff;
  padding: 0.22rem 0.5rem;
  border-radius: 999px;
  background: rgba(124, 37, 255, 0.45);
  border: 1px solid rgba(255, 255, 255, 0.25);
  white-space: nowrap;
}
.t-tag--accent {
  background: rgba(255, 255, 255, 0.12);
}

@media (min-width: 640px) {
  .t-dash-inner {
    max-width: 480px;
  }
}

@media (max-width: 640px) {
  .t-dash {
    min-height: 100dvh;
  }
}
</style>
