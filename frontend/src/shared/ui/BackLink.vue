<script setup lang="ts">
defineProps<{
  to: string
  label?: string
}>()
</script>

<template>
  <RouterLink :to="to" class="back-link">{{ label ?? '← Назад' }}</RouterLink>
</template>

<style scoped>
.back-link {
  display: inline-block;
  margin-bottom: 1rem;
  color: rgba(255, 255, 255, 0.85);
  font-size: 0.9rem;
  font-weight: 500;
  text-decoration: none;
}
.back-link:hover {
  color: #fff;
  text-decoration: none;
}
</style>
