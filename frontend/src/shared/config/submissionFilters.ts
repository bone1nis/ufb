/** Общие опции фильтров списков сдач (студент / преподаватель). */
export const SUBMISSION_DIRECTION_OPTS = [
  { value: 'frontend', label: 'Frontend' },
  { value: 'backend', label: 'Backend' },
  { value: 'ux-ui', label: 'Дизайн' },
] as const

export const SUBMISSION_PROJECT_OPTS = ['ПАЗЛ', 'КОД'] as const
export const SUBMISSION_COURSE_OPTS = [1, 2, 3] as const
