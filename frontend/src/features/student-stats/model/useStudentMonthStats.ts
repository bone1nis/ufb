import { ref, onMounted } from 'vue'
import api from '@/shared/api/axios'
import type { StatItem } from '@/shared/ui/ProgressStatsCard.vue'

interface StudentMonthStats {
  submissions_this_month: number
  avg_score: number | null
  success_percent: number
}

export function useStudentMonthStats() {
  const stats = ref<StatItem[]>([
    { label: 'Сдачи за месяц', display: '…', percent: 0 },
    { label: 'Средняя оценка', display: '…', percent: 0 },
    { label: 'Успеваемость',   display: '…', percent: 0 },
  ])

  onMounted(async () => {
    try {
      const { data } = await api.get<StudentMonthStats>('/api/student/stats')
      stats.value = [
        {
          label:   'Сдачи за месяц',
          display: String(data.submissions_this_month),
          percent: Math.min(data.submissions_this_month * 10, 100),
        },
        {
          label:   'Средняя оценка',
          display: data.avg_score != null ? String(data.avg_score) : '—',
          percent: data.avg_score != null ? Math.min(100, Math.max(0, data.avg_score)) : 0,
        },
        {
          label:   'Успеваемость',
          display: data.submissions_this_month > 0 ? `${data.success_percent}%` : '—',
          percent: data.success_percent,
        },
      ]
    } catch {
      stats.value = [
        { label: 'Сдачи за месяц', display: '—', percent: 0 },
        { label: 'Средняя оценка', display: '—', percent: 0 },
        { label: 'Успеваемость',   display: '—', percent: 0 },
      ]
    }
  })

  return { stats }
}
