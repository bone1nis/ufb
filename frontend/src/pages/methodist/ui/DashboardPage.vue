<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/features/auth/model/useAuthStore'
import api from '@/shared/api/axios'
import type { Homework } from '@/entities/homework/model/types'
import { fmtDeadline } from '@/shared/lib/date'
import HeroBand from '@/shared/ui/HeroBand.vue'
import ProfileHeader from '@/shared/ui/ProfileHeader.vue'
import leftIcon from '@/assets/icons/left.svg'
import rightIcon from '@/assets/icons/right.svg'

const authStore = useAuthStore()

const homeworks = ref<Homework[]>([])
const loading = ref(true)
const cardIdx = ref(0)
const expanded = ref<Record<number, boolean>>({})

const DESC_PREVIEW_LEN = 120

onMounted(async () => {
  try {
    const { data } = await api.get<Homework[]>('/api/methodist/homeworks')
    homeworks.value = data
  }
  finally {
    loading.value = false
  }
})

function prev() {
  if (cardIdx.value > 0)
    cardIdx.value--
}

function next() {
  if (cardIdx.value < homeworks.value.length - 1)
    cardIdx.value++
}

function toggleExpand(id: number) {
  expanded.value[id] = !expanded.value[id]
}

function needsExpand(hw: Homework): boolean {
  const text = hw.description?.trim() ?? ''
  return text.length > DESC_PREVIEW_LEN
}

function descriptionPreview(hw: Homework): string {
  const text = hw.description?.trim() ?? ''
  if (!text)
    return ''
  if (expanded.value[hw.id] || text.length <= DESC_PREVIEW_LEN)
    return text
  return `${text.slice(0, DESC_PREVIEW_LEN).trimEnd()}…`
}

function editUrl(id: number) {
  return `/methodist/homeworks/new?edit=${id}`
}
</script>

<template>
  <div class="m-dash">
    <HeroBand variant="compact">
      <ProfileHeader
        :name="authStore.user?.name ?? ''"
        :tags="['Методист']"
      />
    </HeroBand>

    <div class="m-dash-inner">
      <RouterLink to="/methodist/homeworks/new" class="m-btn-create">
        Создать новое задание
      </RouterLink>

      <div class="m-section-head">
        <h2 class="m-section-title">Созданные дз</h2>
        <div v-if="homeworks.length > 1" class="m-nav-btns">
          <button type="button" class="m-nav-btn" :disabled="cardIdx === 0" @click="prev">
            <img :src="leftIcon" alt="" width="26" height="26">
          </button>
          <button
            type="button"
            class="m-nav-btn"
            :disabled="cardIdx >= homeworks.length - 1"
            @click="next"
          >
            <img :src="rightIcon" alt="" width="26" height="26">
          </button>
        </div>
      </div>

      <div v-if="loading" class="m-state">Загрузка…</div>
      <div v-else-if="!homeworks.length" class="m-state">
        Заданий ещё нет. Нажми «Создать новое задание» выше.
      </div>
      <div v-else class="m-slider-vp">
        <div class="m-slider-track" :style="{ transform: `translateX(-${cardIdx * 100}%)` }">
          <article v-for="hw in homeworks" :key="hw.id" class="m-hw-card">
            <div class="m-hw-top">
              <span class="m-hw-id">Дз №{{ hw.id }}</span>
              <span v-if="hw.deadline" class="m-hw-deadline">До {{ fmtDeadline(hw.deadline) }}</span>
              <span v-else class="m-hw-deadline m-hw-deadline--soft">Без срока</span>
            </div>

            <p v-if="hw.description" class="m-hw-desc" :class="{ 'm-hw-desc--open': expanded[hw.id] }">
              {{ descriptionPreview(hw) }}
            </p>
            <p v-else class="m-hw-desc m-hw-desc--empty">Описание не указано</p>

            <button
              v-if="needsExpand(hw)"
              type="button"
              class="m-expand"
              @click="toggleExpand(hw.id)"
            >
              {{ expanded[hw.id] ? 'Свернуть' : 'Развернуть' }}
              <span class="m-expand-chev" :class="{ 'm-expand-chev--up': expanded[hw.id] }">▾</span>
            </button>

            <RouterLink :to="editUrl(hw.id)" class="m-btn-edit">
              Редактировать
            </RouterLink>
          </article>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.m-dash {
  min-height: calc(100dvh - 58px);
  padding: 0 0 2.5rem;
}

.m-dash-inner {
  max-width: 420px;
  margin: 0 auto;
  padding: 1.35rem 1rem 0;
}

.m-btn-create {
  display: block;
  width: 100%;
  margin-bottom: 1.25rem;
  padding: 0.72rem 1rem;
  border-radius: 999px;
  border: 1px solid rgba(255, 255, 255, 0.45);
  background: rgba(255, 255, 255, 0.12);
  color: #fff;
  font-size: 1rem;
  font-weight: 500;
  text-align: center;
  text-decoration: none;
  transition: background 0.15s, border-color 0.15s;
}
.m-btn-create:hover {
  background: rgba(255, 255, 255, 0.18);
  text-decoration: none;
}

.m-section-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
  margin-bottom: 0.85rem;
}
.m-section-title {
  margin: 0;
  font-size: 1.15rem;
  font-weight: 700;
  color: #fff;
}
.m-nav-btns {
  display: flex;
  gap: 0.4rem;
  flex-shrink: 0;
}
.m-nav-btn {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  border: none;
  background: rgba(255, 255, 255, 0.12);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.15s, opacity 0.15s;
}
.m-nav-btn:hover:not(:disabled) {
  background: rgba(255, 255, 255, 0.22);
}
.m-nav-btn:disabled {
  opacity: 0.35;
  cursor: not-allowed;
}

.m-state {
  text-align: center;
  color: rgba(255, 255, 255, 0.6);
  padding: 2rem 0.5rem;
  font-size: 0.9rem;
  line-height: 1.5;
}

.m-slider-vp {
  overflow: hidden;
  border-radius: 22px;
}
.m-slider-track {
  display: flex;
  transition: transform 0.35s ease;
}
.m-hw-card {
  min-width: 100%;
  box-sizing: border-box;
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border: 1px solid rgba(255, 255, 255, 0.22);
  border-radius: 22px;
  padding: 1.15rem 1.2rem 1.25rem;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.12);
}

.m-hw-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  margin-bottom: 0.65rem;
}
.m-hw-id {
  font-size: 1.05rem;
  font-weight: 700;
  color: #fff;
}
.m-hw-deadline {
  flex-shrink: 0;
  font-size: 0.72rem;
  font-weight: 600;
  color: #fff;
  padding: 0.22rem 0.55rem;
  border-radius: 999px;
  background: rgba(124, 37, 255, 0.5);
  border: 1px solid rgba(255, 255, 255, 0.32);
}
.m-hw-deadline--soft {
  background: rgba(255, 255, 255, 0.12);
}

.m-hw-desc {
  margin: 0 0 0.5rem;
  font-size: 0.88rem;
  line-height: 1.45;
  color: rgba(255, 255, 255, 0.9);
  word-break: break-word;
}
.m-hw-desc--open {
  white-space: pre-wrap;
}
.m-hw-desc--empty {
  color: rgba(255, 255, 255, 0.45);
  font-size: 0.82rem;
}

.m-expand {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  margin: 0 0 0.85rem;
  padding: 0;
  border: none;
  background: none;
  color: rgba(255, 255, 255, 0.75);
  font-size: 0.82rem;
  font-weight: 500;
  cursor: pointer;
}
.m-expand:hover {
  color: #fff;
}
.m-expand-chev {
  display: inline-block;
  font-size: 0.65rem;
  transition: transform 0.2s;
}
.m-expand-chev--up {
  transform: rotate(180deg);
}

.m-btn-edit {
  display: block;
  width: 100%;
  padding: 0.7rem 1rem;
  border-radius: 999px;
  border: none;
  background: #fff;
  color: #7c25ff;
  font-size: 1rem;
  font-weight: 500;
  text-align: center;
  text-decoration: none;
  transition: opacity 0.15s, transform 0.12s;
}
.m-btn-edit:hover {
  opacity: 0.92;
  text-decoration: none;
  transform: translateY(-1px);
}

@media (min-width: 640px) {
  .m-dash-inner {
    max-width: 480px;
  }
}

@media (max-width: 640px) {
  .m-dash {
    min-height: 100dvh;
  }
}
</style>
