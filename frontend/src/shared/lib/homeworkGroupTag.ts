import type { Homework } from '@/entities/homework/model/types'

export function homeworkGroupTag(hw?: Homework | null): string {
  if (!hw)
    return '—'
  const dirLetter: Record<string, string> = {
    frontend: 'Ф',
    backend: 'Б',
    'ux-ui': 'Д',
  }
  const projLetter: Record<string, string> = {
    ПАЗЛ: 'П',
    КОД: 'К',
  }
  const p = projLetter[hw.project] ?? hw.project.charAt(0)
  const d = dirLetter[hw.direction] ?? hw.direction.charAt(0).toUpperCase()
  return `${p}${d}${hw.course}`
}
