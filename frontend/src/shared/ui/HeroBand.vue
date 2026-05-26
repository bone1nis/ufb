<script setup lang="ts">
withDefaults(defineProps<{
  /** full — студент (выше), compact — преподаватель/методист */
  variant?: 'full' | 'compact'
}>(), {
  variant: 'full',
})
</script>

<template>
  <div class="hero-band" :class="`hero-band--${variant}`">
    <div class="hero-top-inner">
      <slot />
    </div>
  </div>
</template>

<style scoped>
.hero-band {
  width: 100vw;
  margin-left: calc(50% - 50vw);
  margin-right: calc(50% - 50vw);
  padding: 1.35rem 0 32px;
  background: linear-gradient(180deg, #6c38ff 0%, #a586ff 100%);
  border-radius: 0 0 45px 45px;
  box-shadow: 0 21px 117px 0 rgba(0, 0, 0, 0.25);
}
.hero-band--full {
  min-height: 324px;
}
.hero-band--compact {
  min-height: 220px;
}
.hero-top-inner {
  width: 100%;
  max-width: 375px;
  margin: 0 auto;
  padding: 0 1rem;
}

@media (min-width: 900px) {
  .hero-band {
    width: 100%;
    margin-left: 0;
    margin-right: 0;
    border-radius: 24px;
    padding: 2rem 1.35rem 1.5rem;
    min-height: 0;
    background: linear-gradient(180deg, #6c38ff 0%, #a586ff 100%);
    box-shadow: 0 21px 117px 0 rgba(0, 0, 0, 0.25);
  }
}

@media (max-width: 640px) {
  .hero-band {
    padding-top: var(--layout-header-offset-mobile);
  }
}
</style>
