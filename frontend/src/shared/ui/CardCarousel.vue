<script setup lang="ts">
import leftIcon from '@/assets/icons/left.svg'
import rightIcon from '@/assets/icons/right.svg'

const props = defineProps<{
  title: string
  count: number
}>()

const index = defineModel<number>({ default: 0 })

function prev() {
  if (index.value > 0)
    index.value--
}

function next() {
  if (index.value < props.count - 1)
    index.value++
}
</script>

<template>
  <div class="carousel">
    <div class="carousel-head">
      <span class="section-title">{{ title }}</span>
      <div v-if="count > 1" class="nav-btns">
        <button type="button" class="nav-btn" :disabled="index === 0" @click="prev">
          <img :src="leftIcon" alt="">
        </button>
        <button type="button" class="nav-btn" :disabled="index >= count - 1" @click="next">
          <img :src="rightIcon" alt="">
        </button>
        </div>
    </div>
    <div class="carousel-body">
      <slot :index="index" />
    </div>
  </div>
</template>

<style scoped>
.carousel-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 0.9rem;
}
.section-title {
  font-size: 1.2rem;
  font-weight: 700;
  color: #fff;
}
.nav-btns {
  display: flex;
  gap: 0.4rem;
}
.nav-btn {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.15);
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.15s;
}
.nav-btn:hover:not(:disabled) {
  background: rgba(255, 255, 255, 0.28);
}
.nav-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}
.nav-btn img {
  width: 26px;
  height: 26px;
}

@media (min-width: 900px) {
  .section-title {
    font-size: 1.35rem;
  }
}
</style>
