import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/features/auth/model/useAuthStore'
import { setPageMeta } from '@/shared/lib/pageMeta'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', redirect: '/login' },
    {
      path: '/login',
      component: () => import('@/pages/login/ui/LoginPage.vue'),
      meta: { guest: true, title: 'Вход' },
    },

    {
      path: '/student',
      component: () => import('@/widgets/layout/ui/AppLayout.vue'),
      meta: { role: 'student' },
      children: [
        { path: '', component: () => import('@/pages/student/ui/DashboardPage.vue'), meta: { title: 'Кабинет' } },
        { path: 'due', component: () => import('@/pages/student/ui/DueHomeworkPage.vue'), meta: { title: 'К сдаче' } },
        { path: 'submissions/:submissionId(\\d+)/edit', name: 'student-submit-edit', component: () => import('@/pages/student/ui/SubmitPage.vue'), meta: { title: 'Редактирование сдачи' } },
        { path: 'submissions', component: () => import('@/pages/student/ui/SubmissionsPage.vue'), meta: { title: 'Мои сдачи' } },
        { path: 'submit', component: () => import('@/pages/student/ui/SubmitPage.vue'), meta: { title: 'Сдать ДЗ' } },
        { path: 'submissions/:id(\\d+)', component: () => import('@/pages/student/ui/SubmissionDetailPage.vue'), meta: { title: 'Сдача' } },
      ],
    },

    {
      path: '/teacher',
      component: () => import('@/widgets/layout/ui/AppLayout.vue'),
      meta: { role: 'teacher' },
      children: [
        { path: '', component: () => import('@/pages/teacher/ui/DashboardPage.vue'), meta: { title: 'Кабинет' } },
        { path: 'submissions', component: () => import('@/pages/teacher/ui/SubmissionsPage.vue'), meta: { title: 'Сдачи' } },
        { path: 'submissions/:id', component: () => import('@/pages/teacher/ui/SubmissionDetailPage.vue'), meta: { title: 'Проверка сдачи' } },
      ],
    },

    {
      path: '/methodist',
      component: () => import('@/widgets/layout/ui/AppLayout.vue'),
      meta: { role: 'methodist' },
      children: [
        { path: '', component: () => import('@/pages/methodist/ui/DashboardPage.vue'), meta: { title: 'Кабинет' } },
        { path: 'homeworks/new', component: () => import('@/pages/methodist/ui/HomeworkNewPage.vue'), meta: { title: 'Создать задание' } },
      ],
    },
  ],
})

router.afterEach((to) => {
  const title = to.matched
    .map(r => r.meta.title)
    .filter((t): t is string => typeof t === 'string')
    .pop()
  setPageMeta(title)
})

router.beforeEach(async (to) => {
  const authStore = useAuthStore()

  if (!authStore.initialized) {
    await authStore.fetchMe()
  }

  if (to.meta.guest) {
    if (authStore.user) return roleHome(authStore.user.role)
    return true
  }

  if (!authStore.user) return '/login'

  if (to.meta.role && authStore.user.role !== to.meta.role) {
    return roleHome(authStore.user.role)
  }

  return true
})

export function roleHome(role: string) {
  const map: Record<string, string> = {
    student: '/student',
    teacher: '/teacher',
    methodist: '/methodist',
  }
  return map[role] ?? '/login'
}

export default router
