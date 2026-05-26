import type { Submission } from '@/entities/submission/model/types'

export function submissionScore(sub: Submission): number | null {
  if (sub.status !== 'graded' || !sub.grade)
    return null
  const raw = sub.grade.score
  if (!Number.isFinite(raw))
    return null
  return Math.min(Math.max(0, raw), 100)
}
