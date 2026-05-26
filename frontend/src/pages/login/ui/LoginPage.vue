<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { NInput, NButton, NAlert, NSpace, NFormItem } from 'naive-ui'
import { useAuthStore } from '@/features/auth/model/useAuthStore'
import { roleHome } from '@/app/router'

const router    = useRouter()
const authStore = useAuthStore()

const email    = ref('')
const password = ref('')
const error    = ref('')

async function submit() {
  error.value = ''
  try {
    const user = await authStore.login(email.value, password.value)
    router.push(roleHome(user.role))
  } catch (e: any) {
    error.value = e?.response?.data?.message ?? 'Ошибка авторизации'
  }
}
</script>

<template>
  <div class="login-wrap">
    <div class="glass-card">
      <p class="welcome">Добро пожаловать!</p>

      <n-space vertical :size="16" style="width: 100%">
        <n-form-item label="Email" label-placement="top" class="glass-item">
          <n-input
            v-model:value="email"
            type="text"
            size="large"
            placeholder="email@example.com"
            :input-props="{ autocomplete: 'email', type: 'email' }"
            @keydown.enter.prevent="submit"
          />
        </n-form-item>

        <n-form-item label="Пароль" label-placement="top" class="glass-item">
          <n-input
            v-model:value="password"
            type="password"
            size="large"
            show-password-on="click"
            placeholder="••••••••"
            :input-props="{ autocomplete: 'current-password' }"
            @keydown.enter.prevent="submit"
          />
        </n-form-item>

        <n-alert v-if="error" type="error" :bordered="false" round>
          {{ error }}
        </n-alert>

        <n-button
          type="primary"
          size="large"
          block
          round
          :loading="authStore.loading"
          @click="submit"
        >
          Войти
        </n-button>
      </n-space>

      <p class="demo-hint">
        demo: student@demo.ru / teacher@demo.ru / methodist@demo.ru<br>пароль: password
      </p>
    </div>
  </div>
</template>

<style scoped>
.login-wrap {
  min-height: 100vh;
  background-image: url('@/assets/bg/gradient.png');
  background-repeat: repeat-x;
  background-size: auto 100vh;
  background-color: #5B21B6;
  background-position: top left;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1.5rem;
}

.glass-card {
  width: 100%;
  max-width: 360px;
  background: rgba(255, 255, 255, 0.14);
  backdrop-filter: blur(28px);
  -webkit-backdrop-filter: blur(28px);
  border: 1px solid rgba(255, 255, 255, 0.22);
  border-radius: 24px;
  padding: 2.4rem 2rem;
  box-shadow: 0 12px 48px rgba(0, 0, 0, 0.25);
}

.welcome {
  text-align: center;
  font-size: 1.55rem;
  font-weight: 700;
  color: #fff;
  margin-bottom: 1.75rem;
  letter-spacing: .01em;
}

.demo-hint {
  margin-top: 1.25rem;
  text-align: center;
  font-size: .75rem;
  color: rgba(255, 255, 255, 0.5);
  line-height: 1.6;
}

:deep(.glass-item .n-form-item-label) {
  color: rgba(255, 255, 255, 0.85) !important;
  font-weight: 500;
}

:deep(.glass-item .n-input) {
  --n-color: rgba(255, 255, 255, 0.1);
  --n-color-focus: rgba(255, 255, 255, 0.12);
  --n-border: 1px solid rgba(255, 255, 255, 0.22);
  --n-border-hover: 1px solid rgba(255, 255, 255, 0.3);
  --n-border-focus: 1px solid rgba(255, 255, 255, 0.36);
  --n-box-shadow-focus: 0 0 0 1px rgba(255, 255, 255, 0.12);
  --n-text-color: rgba(255, 255, 255, 0.95);
  --n-placeholder-color: rgba(255, 255, 255, 0.45);
  --n-caret-color: #fff;
}

:deep(.glass-item .n-input__input-el:-webkit-autofill),
:deep(.glass-item .n-input__input-el:-webkit-autofill:hover),
:deep(.glass-item .n-input__input-el:-webkit-autofill:active),
:deep(.glass-item .n-input__input-el:-webkit-autofill:focus),
:deep(.glass-item .n-input__input-el:-internal-autofill-selected) {
  -webkit-box-shadow: 0 0 0 1000px rgba(255, 255, 255, 0.1) inset !important;
  box-shadow: 0 0 0 1000px rgba(255, 255, 255, 0.1) inset !important;
  -webkit-text-fill-color: rgba(255, 255, 255, 0.95) !important;
  caret-color: #fff !important;
  transition: background-color 99999s ease-out 0s;
  outline: none !important;
}

:deep(.glass-item .n-input__input-el:autofill),
:deep(.glass-item .n-input__input-el:autofill:focus) {
  -webkit-box-shadow: 0 0 0 1000px rgba(255, 255, 255, 0.1) inset !important;
  box-shadow: 0 0 0 1000px rgba(255, 255, 255, 0.1) inset !important;
  -webkit-text-fill-color: rgba(255, 255, 255, 0.95) !important;
  caret-color: #fff !important;
  outline: none !important;
}

@media (min-width: 768px) {
  .glass-card { max-width: 400px; padding: 2.75rem 2.5rem; }
  .welcome { font-size: 1.75rem; }
}
</style>
