<script setup lang="ts">
export interface StatItem {
  label: string
  display: string
  percent: number
}

defineProps<{
  items: StatItem[]
}>()
</script>

<template>
  <div class="stats-card">
    <div v-for="item in items" :key="item.label" class="stat-row">
      <div class="stat-meta">
        <span class="stat-label">{{ item.label }}</span>
        <span class="stat-val">{{ item.display }}</span>
      </div>
      <div class="stat-track">
        <div class="stat-fill" :style="{ width: item.percent + '%' }" />
      </div>
    </div>
  </div>
</template>

<style scoped>
.stats-card {
  background: rgba(255, 255, 255, 0.14);
  backdrop-filter: blur(12px);
  border: 1px solid rgba(255, 255, 255, 0.22);
  border-radius: 18px;
  padding: 1.1rem 1.2rem;
  margin-bottom: 0.85rem;
  display: flex;
  flex-direction: column;
  gap: 0.85rem;
}
.stat-meta {
  display: flex;
  justify-content: space-between;
  color: #fff;
  font-size: 0.84rem;
  margin-bottom: 0.3rem;
}
.stat-label {
  font-weight: 400;
  opacity: 0.85;
}
.stat-val {
  font-weight: 700;
}
.stat-track {
  height: 5px;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 999px;
  overflow: hidden;
}
.stat-fill {
  height: 100%;
  background: #fff;
  border-radius: 999px;
  transition: width 0.5s ease;
}
</style>
