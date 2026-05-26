<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import type { Submission } from '@/entities/submission/model/types'
import SubmissionBriefCard from '@/entities/submission/ui/SubmissionBriefCard.vue'
import CardCarousel from '@/shared/ui/CardCarousel.vue'

defineProps<{
  submissions: Submission[]
  loading: boolean
}>()

const router = useRouter()
const cardIdx = ref(0)
</script>

<template>
  <div v-if="loading" class="placeholder">Загрузка…</div>
  <div v-else-if="!submissions.length" class="placeholder">
    Вы ещё ничего не сдавали.<br>
    <RouterLink to="/student/submit" class="placeholder-link">Сдать ДЗ →</RouterLink>
  </div>
  <CardCarousel v-else v-model="cardIdx" title="Сдано" :count="submissions.length">
    <div class="slider-vp">
      <div class="slider-track" :style="{ transform: `translateX(-${cardIdx * 100}%)` }">
        <SubmissionBriefCard
          v-for="sub in submissions"
          :key="sub.id"
          :submission="sub"
          @detail="router.push(`/student/submissions/${sub.id}`)"
        />
      </div>
    </div>
  </CardCarousel>
</template>

<style scoped>
.slider-vp {
  overflow: hidden;
  border-radius: 20px;
}
.slider-track {
  display: flex;
  transition: transform 0.35s ease;
}
.placeholder {
  text-align: center;
  color: rgba(255, 255, 255, 0.7);
  padding: 2rem;
  background: rgba(255, 255, 255, 0.08);
  border-radius: 20px;
  line-height: 1.6;
}
.placeholder-link {
  color: #fff;
  text-decoration: underline;
}
</style>
