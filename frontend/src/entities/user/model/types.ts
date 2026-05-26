export type UserRole = 'student' | 'teacher' | 'methodist'

export interface User {
  id: number
  name: string
  email: string
  role: UserRole
  vk_user_id?: string | null
}

export const roleLabel: Record<UserRole, string> = {
  student: 'Студент',
  teacher: 'Преподаватель',
  methodist: 'Методист',
}
