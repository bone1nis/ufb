<script setup lang="ts">
import checkIcon from '@/assets/icons/check.svg'
import {
  SUBMISSION_COURSE_OPTS,
  SUBMISSION_DIRECTION_OPTS,
} from '@/shared/config/submissionFilters'

const show = defineModel<boolean>({ required: true })

defineProps<{
  tempDirections: string[]
  tempCourses: number[]
}>()

const emit = defineEmits<{
  apply: []
  toggleDir: [value: string, on: boolean]
  toggleCourse: [value: number, on: boolean]
}>()

function onDir(e: Event, v: string) {
  emit('toggleDir', v, (e.target as HTMLInputElement).checked)
}
function onCourse(e: Event, v: number) {
  emit('toggleCourse', v, (e.target as HTMLInputElement).checked)
}

function close() {
  show.value = false
}

function onApply() {
  emit('apply')
  show.value = false
}
</script>

<template>
  <Teleport to="body">
    <div v-if="show" class="popup-overlay" @click.self="close">
      <div class="popup-sheet popup-sheet--filters">
        <div class="popup-handle" />
        <h2 class="popup-title popup-title--center">Фильтры</h2>

        <p class="filter-section-title">Направление</p>
        <div class="filter-chips">
          <label
            v-for="d in SUBMISSION_DIRECTION_OPTS"
            :key="d.value"
            class="chk"
            :class="{ 'chk--on': tempDirections.includes(d.value) }"
          >
            <input
              type="checkbox"
              class="sr-only"
              :checked="tempDirections.includes(d.value)"
              @change="onDir($event, d.value)"
            >
            <span class="chk-box">
              <img :src="checkIcon" alt="" class="chk-check-img" width="12" height="12">
            </span>
            <span>{{ d.label }}</span>
          </label>
        </div>

        <p class="filter-section-title">Курс</p>
        <div class="filter-chips">
          <label
            v-for="c in SUBMISSION_COURSE_OPTS"
            :key="c"
            class="chk"
            :class="{ 'chk--on': tempCourses.includes(c) }"
          >
            <input
              type="checkbox"
              class="sr-only"
              :checked="tempCourses.includes(c)"
              @change="onCourse($event, c)"
            >
            <span class="chk-box">
              <img :src="checkIcon" alt="" class="chk-check-img" width="12" height="12">
            </span>
            <span>{{ c }} курс</span>
          </label>
        </div>

        <button type="button" class="btn-apply" @click="onApply">Применить</button>
      </div>
    </div>
  </Teleport>
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
.popup-sheet--filters {
  max-height: 85vh;
  overflow-y: auto;
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
.popup-title--center {
  text-align: center;
  margin-bottom: 1rem;
}

.filter-section-title {
  font-size: 0.95rem;
  font-weight: 700;
  color: #111827;
  margin: 0.5rem 0 0.65rem;
}
.filter-chips {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem 0.75rem;
  margin-bottom: 1rem;
}
.chk {
  display: inline-flex;
  align-items: center;
  gap: 0.45rem;
  cursor: pointer;
  font-size: 0.9rem;
  color: #374151;
  user-select: none;
}
.chk-box {
  width: 26px;
  height: 26px;
  border-radius: 8px;
  border: 1px solid #aad3ff;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  box-sizing: border-box;
}
.chk-check-img {
  display: none;
  object-fit: contain;
}
.chk--on .chk-check-img {
  display: block;
}
.chk--on .chk-box {
  background: rgba(170, 211, 255, 0.18);
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
  .popup-sheet--filters {
    max-width: 420px;
  }
}
</style>
