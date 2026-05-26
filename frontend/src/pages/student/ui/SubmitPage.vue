<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  NSelect,
  NInput,
  NButton,
  NAlert,
  NSpace,
  NFormItem,
} from 'naive-ui'
import api from '@/shared/api/axios'
import type { Homework } from '@/entities/homework/model/types'
import type { Submission } from '@/entities/submission/model/types'
import downloadIcon from '@/assets/icons/download.svg'
import attachIcon from '@/assets/icons/attach.svg'
import chainIcon from '@/assets/icons/chain.svg'

const route = useRoute()
const router = useRouter()

const homeworks = ref<Homework[]>([])
const homeworkId = ref<number | null>(null)
const files = ref<File[]>([])
const links = ref<string[]>([''])
const error = ref('')
const loading = ref(false)
const dragActive = ref(false)
const pageLoading = ref(true)

/** Редактирование существующей сдачи (только pending) */
const editSubmissionId = computed(() => {
  const name = route.name
  if (name === 'student-submit-edit') {
    const raw = route.params.submissionId
    const n = Number(raw)
    return Number.isFinite(n) ? n : null
  }
  return null
})

const keptFileItems = ref<{ id: number; name: string }[]>([])

const hwOptions = computed(() =>
  homeworks.value.map(h => ({
    label: `${h.title} (${h.project} · ${h.direction} · ${h.course} курс)`,
    value: h.id,
  })),
)

async function loadHomeworksForNew() {
  const { data } = await api.get<Homework[]>('/api/student/homeworks/todo')
  homeworks.value = data
}

async function loadEditMode(submissionId: number) {
  const { data } = await api.get<Submission>(`/api/student/submissions/${submissionId}`)
  if (data.status !== 'pending') {
    error.value = 'Редактировать можно только работу на проверке'
    return
  }
  if (data.homework) {
    homeworks.value = [data.homework]
    homeworkId.value = data.homework.id
  }
  keptFileItems.value = (data.items ?? [])
    .filter(i => i.type === 'file')
    .map(i => ({ id: i.id, name: i.original_name ?? `Файл ${i.id}` }))
  const linkItems = (data.items ?? []).filter(i => i.type === 'link' && i.url)
  links.value = linkItems.length ? linkItems.map(i => i.url!) : ['']
}

onMounted(async () => {
  error.value = ''
  pageLoading.value = true
  try {
    const sid = editSubmissionId.value
    if (sid != null)
      await loadEditMode(sid)
    else
      await loadHomeworksForNew()

    const q = route.query.homework_id
    if (q != null && homeworkId.value == null) {
      const hid = Number(Array.isArray(q) ? q[0] : q)
      if (Number.isFinite(hid) && homeworks.value.some(h => h.id === hid))
        homeworkId.value = hid
    }
  } catch {
    error.value = 'Не удалось загрузить данные'
  } finally {
    pageLoading.value = false
  }
})

watch(
  () => route.fullPath,
  async () => {
    if (route.name !== 'student-submit-edit' && route.path !== '/student/submit')
      return
    pageLoading.value = true
    error.value = ''
    files.value = []
    links.value = ['']
    keptFileItems.value = []
    homeworkId.value = null
    try {
      const sid = editSubmissionId.value
      if (sid != null)
        await loadEditMode(sid)
      else
        await loadHomeworksForNew()

      const q = route.query.homework_id
      if (q != null && homeworkId.value == null) {
        const hid = Number(Array.isArray(q) ? q[0] : q)
        if (Number.isFinite(hid) && homeworks.value.some(h => h.id === hid))
          homeworkId.value = hid
      }
    } finally {
      pageLoading.value = false
    }
  },
)

function onDrop(e: DragEvent) {
  dragActive.value = false
  const dropped = Array.from(e.dataTransfer?.files ?? [])
  files.value.push(...dropped)
}
function onFileInput(e: Event) {
  const input = e.target as HTMLInputElement
  files.value.push(...Array.from(input.files ?? []))
}
function removeFile(i: number) {
  files.value.splice(i, 1)
}
function removeKeptFile(id: number) {
  keptFileItems.value = keptFileItems.value.filter(k => k.id !== id)
}
function addLink() {
  links.value.push('')
}
function removeLink(i: number) {
  links.value.splice(i, 1)
}

async function submit() {
  if (homeworkId.value == null) {
    error.value = 'Выберите задание'
    return
  }
  const validLinks = links.value.map(l => l.trim()).filter(Boolean)
  const hasNewFiles = files.value.length > 0
  const hasKept = keptFileItems.value.length > 0
  if (!hasNewFiles && !hasKept && !validLinks.length) {
    error.value = 'Добавьте файл или ссылку'
    return
  }
  error.value = ''
  loading.value = true
  try {
    const sid = editSubmissionId.value
    if (sid != null) {
      const form = new FormData()
      if (keptFileItems.value.length)
        form.append('keep_item_ids', keptFileItems.value.map(k => k.id).join(','))
      else
        form.append('keep_item_ids', '')
      files.value.forEach(f => form.append('files[]', f))
      validLinks.forEach(l => form.append('links[]', l))
      await api.patch(`/api/student/submissions/${sid}`, form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      router.push(`/student/submissions/${sid}`)
    }
    else {
      const form = new FormData()
      form.append('homework_id', String(homeworkId.value))
      files.value.forEach(f => form.append('files[]', f))
      validLinks.forEach(l => form.append('links[]', l))
      const { data } = await api.post<{ id: number }>('/api/student/submissions', form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      router.push(`/student/submissions/${data.id}`)
    }
  }
  catch (e: unknown) {
    const err = e as { response?: { data?: { message?: string }; status?: number } }
    error.value = err.response?.data?.message ?? 'Ошибка при отправке'
  }
  finally {
    loading.value = false
  }
}

const submitLabel = computed(() => (editSubmissionId.value != null ? 'Сохранить изменения' : 'Отправить'))
</script>

<template>
  <div class="page-wrap">
    <RouterLink :to="editSubmissionId != null ? `/student/submissions/${editSubmissionId}` : '/student/due'" class="back-link">
      ← Назад
    </RouterLink>

    <div v-if="pageLoading" class="state">Загрузка…</div>
    <div v-else class="form-card">
      <form class="hw-form" @submit.prevent="submit">
        <n-space vertical :size="18" style="width: 100%">
          <n-form-item label="Задание" required>
            <n-select
              v-model:value="homeworkId"
              placeholder="Выберите задание"
              :options="hwOptions"
              filterable
              :clearable="editSubmissionId == null"
              :disabled="editSubmissionId != null"
            />
          </n-form-item>

          <n-form-item label="Файлы">
            <div class="files-block">
              <div
                class="dropzone"
                :class="{ active: dragActive }"
                @dragover.prevent="dragActive = true"
                @dragleave="dragActive = false"
                @drop.prevent="onDrop"
                @click="($refs.fileInput as HTMLInputElement).click()"
              >
                <img :src="downloadIcon" alt="" class="dz-cloud" width="48" height="44">
                <p class="dz-title">Перетащите файл сюда</p>
                <p class="dz-hint">или нажмите, чтобы выбрать с устройства</p>
                <input ref="fileInput" type="file" multiple style="display:none" @change="onFileInput">
              </div>
              <div v-if="keptFileItems.length" class="file-list">
                <div
                  v-for="k in keptFileItems"
                  :key="k.id"
                  class="file-chip"
                  @click.stop
                >
                  <img :src="attachIcon" alt="" class="file-ico" width="18" height="18">
                  <span class="file-name">{{ k.name }}</span>
                  <n-button quaternary size="small" @click.stop="removeKeptFile(k.id)">×</n-button>
                </div>
              </div>
              <div v-if="files.length" class="file-list">
                <div
                  v-for="(f, i) in files"
                  :key="'n-' + i"
                  class="file-chip"
                  @click.stop
                >
                  <img :src="attachIcon" alt="" class="file-ico" width="18" height="18">
                  <span class="file-name">{{ f.name }}</span>
                  <n-button quaternary size="small" @click.stop="removeFile(i)">×</n-button>
                </div>
              </div>
            </div>
          </n-form-item>

          <n-form-item label="Ссылки">
            <n-space vertical :size="8" style="width: 100%">
              <div v-for="(_, i) in links" :key="i" class="link-row">
                <div class="link-ico-wrap" aria-hidden="true">
                  <img :src="chainIcon" alt="" width="18" height="18">
                </div>
                <n-input v-model:value="links[i]" type="text" placeholder="https://..." />
                <n-button quaternary @click="removeLink(i)">×</n-button>
              </div>
              <n-button dashed block @click="addLink">+ Добавить ссылку</n-button>
            </n-space>
          </n-form-item>

          <n-alert v-if="error" type="error">{{ error }}</n-alert>

          <n-button
            type="primary"
            size="large"
            block
            round
            :loading="loading"
            attr-type="submit"
          >
            {{ submitLabel }}
          </n-button>
        </n-space>
      </form>
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
    max-width: 48rem;
    padding: 0 1.25rem 2.5rem;
  }
}

@media (min-width: 1100px) {
  .page-wrap {
    max-width: 52rem;
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

.state {
  text-align: center;
  color: rgba(255, 255, 255, 0.65);
  padding: 2rem;
}

.form-card {
  width: 100%;
  background: rgba(255, 255, 255, 0.11);
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

.hw-form {
  width: 100%;
}

.files-block {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  width: 100%;
}

.dropzone {
  width: 100%;
  min-height: 168px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 0.35rem;
  padding: 1.35rem 1rem 1.5rem;
  text-align: center;
  cursor: pointer;
  border: 2px dashed rgba(255, 255, 255, 0.42);
  border-radius: 28px;
  background: rgba(255, 255, 255, 0.06);
  transition: border-color 0.2s, background 0.2s, box-shadow 0.2s;
}
.dropzone.active {
  border-color: rgba(255, 255, 255, 0.75);
  background: rgba(255, 255, 255, 0.1);
  box-shadow: 0 0 0 3px rgba(124, 37, 255, 0.35);
}

.dz-cloud {
  display: block;
  margin-bottom: 0.15rem;
  filter: drop-shadow(0 4px 12px rgba(0, 0, 0, 0.2));
}
.dz-title {
  margin: 0;
  font-size: 0.95rem;
  font-weight: 600;
  color: rgba(255, 255, 255, 0.95);
}
.dz-hint {
  margin: 0;
  font-size: 0.8rem;
  font-weight: 400;
  color: rgba(255, 255, 255, 0.55);
  max-width: 260px;
  line-height: 1.35;
}

.file-list {
  display: flex;
  flex-direction: column;
  gap: 0.45rem;
  width: 100%;
}
.file-chip {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  width: 100%;
  padding: 0.55rem 0.75rem;
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.18);
}
.file-ico {
  flex-shrink: 0;
  opacity: 0.95;
}
.file-name {
  flex: 1;
  min-width: 0;
  font-size: 0.85rem;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  color: rgba(255, 255, 255, 0.92);
}

.link-row { display: flex; gap: .5rem; align-items: center; width: 100%; }
.link-row :deep(.n-input) { flex: 1; }
.link-ico-wrap {
  flex-shrink: 0;
  width: 40px;
  height: 40px;
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.95);
  display: flex;
  align-items: center;
  justify-content: center;
}

:deep(.n-form-item-label) {
  color: rgba(255, 255, 255, 0.8) !important;
}
</style>
