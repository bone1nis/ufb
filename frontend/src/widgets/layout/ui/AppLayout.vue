<script setup lang="ts">
import { ref, watch, computed, onMounted, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { NDrawer, NDrawerContent, NButton, NDivider } from 'naive-ui'
import { useAuthStore } from '@/features/auth/model/useAuthStore'
import { roleLabel } from '@/entities/user/model/types'
import { roleHome } from '@/app/router'

const router    = useRouter()
const route     = useRoute()
const authStore = useAuthStore()
const menuOpen  = ref(false)

const navLinks: Record<string, { label: string; to: string; icon: string }[]> = {
  student: [
    { label: 'Кабинет',   to: '/student',               icon: '◆' },
    { label: 'К сдаче',   to: '/student/due',         icon: '◇' },
    { label: 'Мои сдачи', to: '/student/submissions', icon: '☰' },
    { label: 'Сдать ДЗ',  to: '/student/submit',      icon: '↑' },
  ],
  teacher: [
    { label: 'Кабинет', to: '/teacher',               icon: '◆' },
    { label: 'Сдачи',   to: '/teacher/submissions', icon: '☰' },
  ],
  methodist: [
    { label: 'Кабинет',    to: '/methodist',                 icon: '◆' },
    { label: 'Создать ДЗ', to: '/methodist/homeworks/new',   icon: '+' },
  ],
}

const links = computed(() => navLinks[authStore.user?.role ?? ''] ?? [])

/** Drawer только на узкой ширине (иначе SSR/resize ломает вёрстку) */
const isNarrow = ref(
  typeof window !== 'undefined' ? window.matchMedia('(max-width: 640px)').matches : true,
)
let mqCleanup: (() => void) | null = null

onMounted(() => {
  const mq = window.matchMedia('(max-width: 640px)')
  const apply = () => {
    isNarrow.value = mq.matches
    if (!mq.matches) menuOpen.value = false
  }
  mq.addEventListener('change', apply)
  apply()
  mqCleanup = () => mq.removeEventListener('change', apply)
})

onUnmounted(() => {
  mqCleanup?.()
})

watch(() => route.fullPath, () => { menuOpen.value = false })

async function handleLogout() {
  menuOpen.value = false
  await authStore.logout()
  router.push('/login')
}
</script>

<template>
  <div class="app-shell">

    <header class="app-header">
      <div class="hdr-inner">

        <div class="hdr-left">
          <nav class="app-nav">
            <RouterLink
              v-for="link in links"
              :key="link.to"
              :to="link.to"
              class="nav-link"
              active-class="nav-link--active"
              exact-active-class="nav-link--active"
            >{{ link.label }}</RouterLink>
          </nav>
        </div>

        <div class="hdr-right">
          <span class="hdr-user">
            {{ authStore.user?.name }}&thinsp;·&thinsp;{{ roleLabel[authStore.user?.role ?? 'student'] }}
          </span>
          <button type="button" class="btn-exit" @click="handleLogout">Выйти</button>
        </div>

        <button
          type="button"
          class="burger"
          :class="{ 'burger--open': menuOpen }"
          aria-label="Меню"
          :aria-expanded="menuOpen"
          @click="menuOpen = !menuOpen"
        >
          <span class="burger-line" />
          <span class="burger-line" />
          <span class="burger-line" />
        </button>
      </div>
    </header>

    <n-drawer
      v-if="isNarrow"
      v-model:show="menuOpen"
      :width="300"
      placement="right"
      display-directive="show"
      :auto-focus="false"
      :trap-focus="false"
      class="app-drawer"
    >
      <n-drawer-content
        body-content-class="drawer-body"
        :native-scrollbar="false"
        closable
      >
        <template #header>
          <RouterLink
            class="drawer-head"
            :to="roleHome(authStore.user?.role ?? 'student')"
            @click="menuOpen = false"
          >
            <div class="drawer-avatar">{{ (authStore.user?.name ?? '?').charAt(0).toUpperCase() }}</div>
            <div class="drawer-user-meta">
              <div class="drawer-name">{{ authStore.user?.name }}</div>
              <div class="drawer-role">{{ roleLabel[authStore.user?.role ?? 'student'] }}</div>
            </div>
          </RouterLink>
        </template>

        <div class="drawer-links">
          <RouterLink
            v-for="link in links"
            :key="link.to"
            :to="link.to"
            class="drawer-link"
            active-class="drawer-link--active"
            exact-active-class="drawer-link--active"
            @click="menuOpen = false"
          >
            <span class="drawer-link-ico">{{ link.icon }}</span>
            <span>{{ link.label }}</span>
          </RouterLink>
        </div>

        <n-divider style="margin: 1rem 0" />

        <n-button block secondary strong @click="handleLogout">
          Выйти из аккаунта
        </n-button>
      </n-drawer-content>
    </n-drawer>

    <main class="app-main">
      <div class="container">
        <RouterView />
      </div>
    </main>

  </div>
</template>

<style scoped>
.app-shell { min-height: 100vh; display: flex; flex-direction: column; }

.app-header {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 100;
  isolation: isolate;
  padding-top: env(safe-area-inset-top, 0px);
  background: transparent;
}

.hdr-inner {
  position: relative;
  z-index: 1;
  max-width: 1080px;
  margin: 0 auto;
  padding: 0 1rem;
  height: 58px;
  display: flex;
  align-items: center;
  gap: 1.25rem;
}

.hdr-left { display: flex; align-items: center; gap: 1rem; flex: 1; min-width: 0; }
.app-nav { display: flex; gap: .2rem; }
.nav-link {
  padding: .32rem .75rem;
  border-radius: 8px;
  font-size: .875rem;
  color: rgba(255,255,255,0.65);
  transition: background .12s, color .12s;
  text-decoration: none;
}
.nav-link:hover { background: rgba(255,255,255,0.1); color: #fff; text-decoration: none; }
.nav-link--active { background: rgba(255,255,255,0.16); color: #fff; font-weight: 600; }

.hdr-right { display: flex; align-items: center; gap: .75rem; flex-shrink: 0; }
.hdr-user {
  font-size: .82rem;
  color: rgba(255,255,255,0.6);
  white-space: nowrap;
}
.btn-exit {
  padding: .32rem .85rem;
  background: rgba(255,255,255,0.12);
  border: 1px solid rgba(255,255,255,0.22);
  border-radius: 8px;
  font-size: .82rem;
  font-weight: 500;
  color: #fff;
  cursor: pointer;
  transition: background .15s;
}
.btn-exit:hover { background: rgba(255,255,255,0.22); }

/* Бургер — только три линии */
.burger {
  display: none;
  flex-direction: column;
  justify-content: center;
  align-items: flex-end;
  gap: 5px;
  width: 44px;
  height: 44px;
  margin-left: auto;
  flex-shrink: 0;
  padding: 0;
  border: none;
  background: transparent;
  box-shadow: none;
  cursor: pointer;
  -webkit-tap-highlight-color: transparent;
}
.burger:hover {
  opacity: 0.8;
}
.burger-line {
  display: block;
  width: 22px;
  height: 2px;
  border-radius: 2px;
  background: #fff;
  transition: transform 0.28s ease, opacity 0.2s ease;
  transform-origin: center;
}
.burger--open .burger-line:nth-child(1) {
  transform: translateY(7px) rotate(45deg);
}
.burger--open .burger-line:nth-child(2) {
  opacity: 0;
}
.burger--open .burger-line:nth-child(3) {
  transform: translateY(-7px) rotate(-45deg);
}

.app-main {
  flex: 1;
  padding: var(--layout-header-offset) 0 0;
  width: 100%;
}

@media (max-width: 640px) {
  /* Контент с полноэкранным фоном тянется под прозрачную шапку — отступ задают страницы */
  .app-main {
    padding-top: 0;
  }
  .hdr-inner {
    height: 52px;
    padding: 0 0.85rem;
  }
  .hdr-left {
    display: none;
  }
  .app-nav {
    display: none;
  }
  .hdr-right {
    display: none;
  }
  .burger {
    display: flex;
  }
}

</style>

<style>
/* Drawer — тот же визуальный язык, что HeroBand (градиент + тень) */
.app-drawer .n-drawer-mask {
  background: rgba(32, 10, 72, 0.48) !important;
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
}

.app-drawer .n-drawer-content-wrapper {
  border-radius: 22px 0 0 22px;
  overflow: hidden;
  border-left: 1px solid rgba(255, 255, 255, 0.22);
  box-shadow:
    -16px 0 64px rgba(0, 0, 0, 0.28),
    0 21px 117px 0 rgba(0, 0, 0, 0.18);
  background: linear-gradient(180deg, #6c38ff 0%, #a586ff 100%);
}

.app-drawer .n-drawer-content {
  background: transparent;
}

.app-drawer .n-drawer-header {
  background: transparent !important;
  border-bottom: 1px solid rgba(255, 255, 255, 0.16) !important;
}

.app-drawer .n-drawer-body,
.app-drawer .n-drawer-body-content-wrapper {
  background: transparent !important;
  backdrop-filter: none !important;
  -webkit-backdrop-filter: none !important;
}

.app-drawer .n-scrollbar-content {
  background: transparent;
}

.app-drawer .n-drawer-header__close {
  color: rgba(255, 255, 255, 0.75) !important;
  border-radius: 50%;
  transition: background 0.15s, color 0.15s;
}

.app-drawer .n-drawer-header__close:hover {
  background: rgba(255, 255, 255, 0.14) !important;
  color: #fff !important;
}

.app-drawer .n-button.n-button--secondary.n-button--block {
  --n-height: 46px !important;
  --n-border-radius: 14px !important;
  --n-color: rgba(255, 255, 255, 0.14) !important;
  --n-color-hover: rgba(255, 255, 255, 0.2) !important;
  --n-color-pressed: rgba(255, 255, 255, 0.12) !important;
  --n-border: 1px solid rgba(255, 255, 255, 0.32) !important;
  --n-border-hover: 1px solid rgba(255, 255, 255, 0.45) !important;
  --n-border-focus: 1px solid rgba(255, 255, 255, 0.45) !important;
  --n-text-color: #fff !important;
  font-weight: 600 !important;
}

.drawer-body {
  padding-top: 0.5rem !important;
}

.drawer-head {
  display: flex;
  align-items: center;
  gap: 0.85rem;
  text-decoration: none;
  color: inherit;
  cursor: pointer;
  border-radius: 14px;
  margin: -0.2rem -0.35rem 0;
  padding: 0.35rem 0.35rem 0.5rem;
  transition: background 0.15s;
}

.drawer-head:hover {
  background: rgba(255, 255, 255, 0.12);
}

.drawer-avatar {
  width: 48px;
  height: 48px;
  border-radius: 16px;
  background: linear-gradient(135deg, #6c38ff, #c4b5fd);
  color: #fff;
  font-weight: 800;
  font-size: 1.15rem;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 6px 24px rgba(0, 0, 0, 0.2);
}

.drawer-user-meta {
  min-width: 0;
}

.drawer-name {
  font-weight: 700;
  font-size: 1.05rem;
  color: #fff;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.drawer-role {
  font-size: 0.8rem;
  color: rgba(255, 255, 255, 0.65);
  margin-top: 2px;
}

.drawer-links {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  margin-top: 0.5rem;
}

.drawer-link {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.85rem 1rem;
  border-radius: 14px;
  color: rgba(255, 255, 255, 0.92);
  text-decoration: none;
  font-weight: 500;
  font-size: 0.95rem;
  transition: background 0.15s, color 0.15s;
}

.drawer-link:hover {
  background: rgba(255, 255, 255, 0.14);
  color: #fff;
}

.drawer-link--active {
  background: rgba(255, 255, 255, 0.18);
  color: #fff;
  box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.22);
}

.drawer-link-ico {
  width: 28px;
  text-align: center;
  opacity: 0.9;
  font-size: 0.9rem;
}
</style>
