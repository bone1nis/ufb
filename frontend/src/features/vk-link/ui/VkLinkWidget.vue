<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useAuthStore } from '@/features/auth/model/useAuthStore'
import { VK_BOT_URL } from '@/shared/config/env'
import type { UserRole } from '@/entities/user/model/types'

const authStore = useAuthStore()

const vkHintText = computed(() => {
  const role = authStore.user?.role as UserRole | undefined
  if (role === 'teacher') {
    return 'Привяжи ВКонтакте, чтобы получать в личные сообщения уведомления об отправке домашних заданий учениками.'
  }
  if (role === 'student') {
    return 'Привяжи ВКонтакте, чтобы получать в личные сообщения уведомления, когда домашнее задание проверено и выставлена оценка.'
  }
  return 'Привяжи ВКонтакте, чтобы получать уведомления в личные сообщения.'
})

const vkId = ref(authStore.user?.vk_user_id ?? '')
const saving = ref(false)
const msg = ref<{ text: string; type: 'ok' | 'err' } | null>(null)
const expanded = ref(false)

watch(
  () => authStore.user?.vk_user_id,
  (val) => { vkId.value = val ?? '' },
  { immediate: true },
)

const isLinked = computed(() => !!authStore.user?.vk_user_id)

const botLink = VK_BOT_URL

async function save() {
  msg.value = null
  saving.value = true
  try {
    await authStore.linkVk(vkId.value.trim() || null)
    msg.value = { text: isLinked.value ? 'ВКонтакте привязан' : 'Привязка удалена', type: 'ok' }
    if (isLinked.value) expanded.value = false
  } catch {
    msg.value = { text: 'Не удалось сохранить', type: 'err' }
  } finally {
    saving.value = false
  }
}

async function unlink() {
  vkId.value = ''
  await save()
}
</script>

<template>
  <div class="vk-widget">
    <button type="button" class="vk-status-bar" @click="expanded = !expanded">
      <div class="vk-status-left">
        <span class="vk-badge" :class="isLinked ? 'vk-badge--linked' : 'vk-badge--empty'">ВК</span>
        <div class="vk-status-text">
          <span class="vk-status-label">Уведомления ВКонтакте</span>
          <span class="vk-status-sub" :class="isLinked ? 'vk-linked' : 'vk-unlinked'">
            {{ isLinked ? `Привязан · id ${authStore.user?.vk_user_id}` : 'Не привязан — нажми, чтобы подключить' }}
          </span>
        </div>
      </div>
      <span class="vk-chevron">{{ expanded ? '▲' : '▼' }}</span>
    </button>

    <div v-if="expanded" class="vk-body">
      <template v-if="!isLinked">
        <p class="vk-hint">
          {{ vkHintText }}
        </p>
        <ol class="vk-steps">
          <li>
            Напиши нашему боту команду&nbsp;<strong>/start</strong>&nbsp;— он покажет твой числовой id
          </li>
          <li>Скопируй id и вставь в поле ниже</li>
        </ol>
        <a :href="botLink" target="_blank" rel="noopener" class="btn btn-vk">
          Открыть бот ВКонтакте →
        </a>
      </template>

      <div class="vk-form">
        <input
          v-model="vkId"
          type="text"
          inputmode="numeric"
          class="input"
          placeholder="Числовой id, например 123456789"
          style="max-width:260px"
        />
        <div class="vk-form-actions">
          <button class="btn btn-primary" :disabled="saving" @click="save">
            {{ saving ? '…' : isLinked ? 'Обновить' : 'Привязать' }}
          </button>
          <button v-if="isLinked" class="btn btn-outline" :disabled="saving" @click="unlink">
            Отвязать
          </button>
        </div>
      </div>

      <p v-if="msg" class="vk-msg" :class="msg.type === 'ok' ? 'vk-msg--ok' : 'vk-msg--err'">
        {{ msg.text }}
      </p>
    </div>
  </div>
</template>

<style scoped>
.vk-widget {
  border: 1px solid rgba(255,255,255,0.14);
  border-radius: 14px;
  background: rgba(255,255,255,0.1);
  backdrop-filter: blur(14px);
  -webkit-backdrop-filter: blur(14px);
  margin-bottom: 1rem;
  overflow: hidden;
}

.vk-status-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  padding: 0.65rem 0.9rem;
  min-height: 3.25rem;
  box-sizing: border-box;
  background: none;
  border: none;
  cursor: pointer;
  text-align: left;
  gap: 0.75rem;
}

.vk-status-left {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  min-width: 0;
  flex: 1;
}

.vk-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border-radius: 8px;
  font-weight: 700;
  font-size: 0.8rem;
  flex-shrink: 0;
  line-height: 1;
}

.vk-badge--linked {
  background: rgba(0, 119, 255, 0.92);
  color: #fff;
}

.vk-badge--empty {
  background: rgba(255, 255, 255, 0.15);
  color: rgba(255, 255, 255, 0.75);
}

.vk-status-text {
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
  justify-content: center;
  min-width: 0;
}

.vk-status-label {
  font-size: 0.875rem;
  font-weight: 600;
  color: #fff;
  line-height: 1.25;
}

.vk-status-sub {
  font-size: 0.75rem;
  font-weight: 400;
  line-height: 1.3;
  letter-spacing: 0.01em;
}

.vk-linked {
  color: rgba(255, 255, 255, 0.62);
}
.vk-unlinked {
  color: rgba(255, 255, 255, 0.48);
}

.vk-chevron {
  color: rgba(255, 255, 255, 0.45);
  font-size: 0.7rem;
  flex-shrink: 0;
  line-height: 1;
}

.vk-body {
  padding: 0 1rem 1rem;
  border-top: 1px solid var(--c-border);
}

.vk-hint {
  margin-top: .75rem;
  font-size: .875rem;
  color: var(--c-muted);
}

.vk-steps {
  margin: .5rem 0 .75rem 1.2rem;
  font-size: .875rem;
  display: flex;
  flex-direction: column;
  gap: .3rem;
}

.btn-vk {
  display: inline-block;
  background: #0077ff;
  color: #fff;
  padding: .45rem 1rem;
  border-radius: 8px;
  font-size: .875rem;
  font-weight: 600;
  text-decoration: none;
  transition: background .15s;
  border: none;
  cursor: pointer;
  margin-bottom: .75rem;
}

.btn-vk:hover { background: #0060cc; text-decoration: none; }

.vk-form {
  display: flex;
  flex-wrap: wrap;
  gap: .5rem;
  align-items: center;
  margin-top: .75rem;
}

.vk-form-actions {
  display: flex;
  gap: .5rem;
}

.vk-msg {
  margin-top: .5rem;
  font-size: .8rem;
}

.vk-msg--ok { color: #16a34a; }
.vk-msg--err { color: #dc2626; }
</style>
