<script setup lang="ts">
import cameraIcon from '@/assets/icons/camera.svg'

defineProps<{
  name: string
  tags?: string[]
}>()
</script>

<template>
  <div class="profile">
    <div class="avatar" aria-hidden="true">
      <img :src="cameraIcon" alt="" class="avatar-ico">
    </div>
    <div class="profile-info">
      <div class="profile-name">{{ name }}</div>
      <div v-if="tags?.length" class="profile-tags">
        <span v-for="tag in tags" :key="tag" class="tag">{{ tag }}</span>
      </div>
      <slot name="tags" />
    </div>
  </div>
</template>

<style scoped>
.profile {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 1rem;
}
.avatar {
  width: 76px;
  height: 76px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.18);
  border: 2px solid rgba(255, 255, 255, 0.3);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.avatar-ico {
  width: 40px;
  height: 40px;
}
.profile-name {
  font-size: 1.35rem;
  font-weight: 700;
  color: #fff;
}
.profile-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 0.45rem;
  margin-top: 0.4rem;
}
.tag {
  background: transparent;
  color: #fff;
  border: 1px solid rgba(255, 255, 255, 0.55);
  border-radius: 999px;
  padding: 0.12rem 0.65rem;
  font-size: 0.75rem;
  font-weight: 500;
}

@media (min-width: 900px) {
  .profile-name {
    font-size: 1.5rem;
  }
}
</style>
