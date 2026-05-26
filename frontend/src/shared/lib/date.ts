export function fmtDate(d: string) {
  return new Date(d).toLocaleDateString('ru')
}

export function fmtDateTime(d: string) {
  return new Date(d).toLocaleString('ru')
}

export function fmtDeadline(d: string | null) {
  return d ? new Date(d).toLocaleDateString('ru') : 'без срока'
}
