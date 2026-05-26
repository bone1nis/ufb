<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  NForm,
  NFormItem,
  NInput,
  NSelect,
  NDatePicker,
  NButton,
  NAlert,
  NSpace,
} from 'naive-ui'
import api from '@/shared/api/axios'

const route = useRoute()
const router = useRouter()

const editId = computed(() => {
  const raw = route.query.edit
  const n = Number(Array.isArray(raw) ? raw[0] : raw)
  return Number.isFinite(n) && n > 0 ? n : null
})

const isEdit = computed(() => editId.value != null)
const pageLoading = ref(false)

const form = ref({
  title: '',
  description: '',
  project: null as string | null,
  direction: null as string | null,
  course: null as string | null,
})
const deadlineMs = ref<number | null>(null)
const error   = ref('')
const loading = ref(false)

const projectOptions = [
  { label: 'КОД', value: 'КОД' },
  { label: 'ПАЗЛ', value: 'ПАЗЛ' },
]
const directionOptions = [
  { label: 'Frontend', value: 'frontend' },
  { label: 'Backend', value: 'backend' },
  { label: 'UX/UI', value: 'ux-ui' },
]
const courseOptions = [
  { label: '1 курс', value: '1' },
  { label: '2 курс', value: '2' },
  { label: '3 курс', value: '3' },
]

onMounted(async () => {
  const id = editId.value
  if (id == null)
    return
  pageLoading.value = true
  error.value = ''
  try {
    const { data } = await api.get<{
      title: string
      description?: string | null
      project: string
      direction: string
      course: number
      deadline?: string | null
    }>(`/api/methodist/homeworks/${id}`)
    form.value = {
      title: data.title,
      description: data.description ?? '',
      project: data.project,
      direction: data.direction,
      course: String(data.course),
    }
    deadlineMs.value = data.deadline ? new Date(data.deadline).getTime() : null
  }
  catch {
    error.value = 'Не удалось загрузить задание'
  }
  finally {
    pageLoading.value = false
  }
})

async function submit() {
  error.value   = ''
  loading.value = true
  try {
    const course = Number(form.value.course ?? '')
    const body = {
      title:       form.value.title,
      description: form.value.description || null,
      project:     form.value.project ?? '',
      direction:   form.value.direction ?? '',
      course:      Number.isFinite(course) ? course : 0,
      deadline:    deadlineMs.value ? new Date(deadlineMs.value).toISOString() : null,
    }
    if (isEdit.value && editId.value != null)
      await api.put(`/api/methodist/homeworks/${editId.value}`, body)
    else
      await api.post('/api/methodist/homeworks', body)
    router.push('/methodist')
  } catch (e: any) {
    const errors = e?.response?.data?.errors
    error.value = errors
      ? Object.values(errors).flat().join(', ')
      : (e?.response?.data?.message ?? 'Ошибка')
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="page-wrap">

    <RouterLink to="/methodist" class="back-link">← Кабинет</RouterLink>

    <div v-if="pageLoading" class="state-msg">Загрузка…</div>
    <div v-else class="form-card">
      <n-form @submit.prevent="submit">
        <n-space vertical :size="18" style="width: 100%">
          <n-form-item label="Название" required>
            <n-input
              v-model:value="form.title"
              placeholder="Верстка лендинга"
              maxlength="200"
              show-count
            />
          </n-form-item>

          <n-form-item label="Описание">
            <n-input
              v-model:value="form.description"
              type="textarea"
              placeholder="Что нужно сделать..."
              :autosize="{ minRows: 3, maxRows: 8 }"
            />
          </n-form-item>

          <div class="row-2">
            <n-form-item label="Проект" required>
              <n-select
                v-model:value="form.project"
                placeholder="Выберите проект"
                :options="projectOptions"
                clearable
              />
            </n-form-item>
            <n-form-item label="Направление" required>
              <n-select
                v-model:value="form.direction"
                placeholder="Направление"
                :options="directionOptions"
                clearable
              />
            </n-form-item>
          </div>

          <div class="row-2">
            <n-form-item label="Курс" required>
              <n-select
                v-model:value="form.course"
                placeholder="Курс"
                :options="courseOptions"
                clearable
              />
            </n-form-item>
            <n-form-item label="Срок сдачи">
              <n-date-picker
                v-model:value="deadlineMs"
                type="datetime"
                clearable
                style="width: 100%"
                placeholder="Не обязательно"
              />
            </n-form-item>
          </div>

          <n-alert v-if="error" type="error">{{ error }}</n-alert>

          <n-button type="primary" size="large" block round :loading="loading" attr-type="submit">
            {{ isEdit ? 'Сохранить изменения' : 'Создать задание' }}
          </n-button>
        </n-space>
      </n-form>
    </div>

  </div>
</template>

<style scoped>
.page-wrap {
  width: 100%;
  max-width: 42rem;
  margin: 0 auto;
  padding: 0 1rem 2rem;
  box-sizing: border-box;
}

@media (max-width: 640px) {
  .page-wrap {
    padding-top: calc(var(--layout-header-offset-mobile) + 0.35rem);
  }
}

@media (min-width: 768px) {
  .page-wrap {
    max-width: 52rem;
    padding: 0 1.25rem 2.5rem;
  }
}

@media (min-width: 1100px) {
  .page-wrap {
    max-width: 58rem;
  }
}

.back-link {
  display: inline-block;
  font-size: 0.875rem;
  color: rgba(255, 255, 255, 0.65);
  margin-top: 0.5rem;
  margin-bottom: 0.75rem;
  text-decoration: none;
}
.back-link:hover {
  color: #fff;
}

.state-msg {
  text-align: center;
  color: rgba(255, 255, 255, 0.6);
  padding: 2rem 0;
}

.form-card {
  width: 100%;
  background: rgba(255, 255, 255, 0.08);
  backdrop-filter: blur(16px);
  border: 1px solid rgba(255, 255, 255, 0.14);
  border-radius: 20px;
  padding: 1.5rem;
}

@media (min-width: 768px) {
  .form-card {
    padding: 1.75rem 2rem;
  }
}

.row-2 {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
  align-items: start;
}
.row-2 :deep(.n-form-item) {
  min-width: 0;
}

@media (max-width: 640px) {
  .row-2 {
    grid-template-columns: 1fr;
  }
}
</style>
