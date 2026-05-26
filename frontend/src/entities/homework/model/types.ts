export interface Homework {
  id: number
  title: string
  description?: string | null
  project: string
  direction: string
  course: number
  deadline?: string | null
  created_at?: string
  created_by?: number
}

export const directionLabel: Record<string, string> = {
  frontend: 'Frontend',
  backend: 'Backend',
  'ux-ui': 'UX/UI',
}
