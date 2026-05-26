<script setup lang="ts">
import { ref, onMounted } from 'vue'
import api from '@/shared/api/axios'
import type { Submission } from '@/entities/submission/model/types'
import StudentDashboardHero from '@/widgets/student-dashboard/ui/StudentDashboardHero.vue'
import StudentSubmissionsCarousel from '@/widgets/student-dashboard/ui/StudentSubmissionsCarousel.vue'

const submissions = ref<Submission[]>([])
const loading = ref(true)

onMounted(async () => {
  try {
    const { data } = await api.get<Submission[]>('/api/student/submissions')
    submissions.value = data
  }
  finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="dash">
    <div class="dash-layout">
      <aside class="dash-aside">
        <StudentDashboardHero />
      </aside>

      <section class="dash-main">
        <div class="dash-inner">
          <StudentSubmissionsCarousel
            :submissions="submissions"
            :loading="loading"
          />
          <RouterLink to="/student/due" class="act-due">Дедлайны ДЗ</RouterLink>
        </div>
      </section>
    </div>
  </div>
</template>

<style scoped>
.dash {
  min-height: calc(100dvh - 58px);
  padding: 0 0 2.5rem;
}
.dash-layout {
  display: block;
}
.dash-aside {
  display: block;
}
.dash-main {
  min-width: 0;
}
.dash-inner {
  max-width: 480px;
  margin: 0 auto;
  padding: 1.35rem 1rem 0;
}
.act-due {
  margin-top: 1.25rem;
  display: block;
  width: 100%;
  text-align: center;
  padding: 0.72rem 1rem;
  border-radius: 14px;
  background: rgba(255, 255, 255, 0.14);
  border: 1px solid rgba(255, 255, 255, 0.32);
  color: #fff;
  font-weight: 600;
  font-size: 0.95rem;
  text-decoration: none;
  transition: background 0.15s;
}
.act-due:hover {
  text-decoration: none;
  background: rgba(255, 255, 255, 0.22);
}

@media (min-width: 900px) {
  .dash {
    padding: 1.25rem 0 2.5rem;
  }
  .dash-layout {
    display: grid;
    grid-template-columns: minmax(300px, 400px) minmax(0, 1fr);
    gap: 1.75rem 2rem;
    align-items: start;
    max-width: 1040px;
    margin: 0 auto;
    padding: 0 1rem;
  }
  .dash-inner {
    max-width: none;
    margin: 0;
    padding: 0;
  }
}

@media (max-width: 640px) {
  .dash {
    min-height: 100dvh;
  }
}
</style>
