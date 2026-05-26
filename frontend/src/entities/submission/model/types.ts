import type { Homework } from '@/entities/homework/model/types'

export type SubmissionStatus = 'pending' | 'graded'

export interface SubmissionItem {
  id: number
  type: 'file' | 'link'
  url?: string | null
  original_name?: string | null
  file_size?: number | null
}

export interface SubmissionGrade {
  score: number
  comment?: string | null
  graded_at: string
  teacher?: { name: string }
}

export interface Submission {
  id: number
  homework_id: number
  status: SubmissionStatus
  submitted_at: string
  homework?: Homework
  student?: { id?: number; name: string; email: string }
  items?: SubmissionItem[]
  grade?: SubmissionGrade | null
}

export const statusLabel: Record<SubmissionStatus, string> = {
  pending: 'На проверке',
  graded: 'Проверено',
}
