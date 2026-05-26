import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '@/shared/api/axios'
import type { User } from '@/entities/user/model/types'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(null)
  const loading = ref(false)
  const initialized = ref(false)

  const isLoggedIn = computed(() => !!user.value)

  async function fetchMe() {
    try {
      const { data } = await api.get('/api/auth/me')
      user.value = data.user
    } catch {
      user.value = null
    } finally {
      initialized.value = true
    }
  }

  async function login(email: string, password: string): Promise<User> {
    loading.value = true
    try {
      const { data } = await api.post('/api/auth/login', { email, password })
      user.value = data.user
      return data.user
    } finally {
      loading.value = false
    }
  }

  async function logout() {
    await api.post('/api/auth/logout')
    user.value = null
  }

  async function linkVk(vkUserId: string | null) {
    const { data } = await api.post('/api/auth/vk', { vk_user_id: vkUserId || null })
    if (user.value) user.value.vk_user_id = data.vk_user_id ?? null
    return data
  }

  return { user, loading, initialized, isLoggedIn, fetchMe, login, logout, linkVk }
})
