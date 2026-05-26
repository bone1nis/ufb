<script setup lang="ts">
import { useAuthStore } from '@/features/auth/model/useAuthStore'
import { useStudentMonthStats } from '@/features/student-stats/model/useStudentMonthStats'
import VkLinkWidget from '@/features/vk-link/ui/VkLinkWidget.vue'
import HeroBand from '@/shared/ui/HeroBand.vue'
import ProfileHeader from '@/shared/ui/ProfileHeader.vue'
import ProgressStatsCard from '@/shared/ui/ProgressStatsCard.vue'

const authStore = useAuthStore()
const { stats } = useStudentMonthStats()
</script>

<template>
  <HeroBand variant="full">
    <ProfileHeader
      :name="authStore.user?.name ?? ''"
      :tags="['Студент', '2 курс']"
    />
    <ProgressStatsCard :items="stats" />
    <VkLinkWidget class="vk-in-hero" />
  </HeroBand>
</template>

<style scoped>
:deep(.vk-in-hero) {
  margin-bottom: 0;
  margin-top: 0.25rem;
  background: rgba(255, 255, 255, 0.18);
  border-color: rgba(255, 255, 255, 0.35);
}
</style>
