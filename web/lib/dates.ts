const LOCALE = 'en-US'

/** Take the date portion of an API datetime ('2026-08-10T00:00:00') or plain ISO date. */
export function isoDate(value: string): string {
  return value.slice(0, 10)
}

export function todayISO(): string {
  const d = new Date()
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`
}

export function pad(n: number): string {
  return String(n).padStart(2, '0')
}

export function fmt(value: string, withYear = true): string {
  const d = new Date(`${isoDate(value)}T00:00`)
  return d.toLocaleDateString(
    LOCALE,
    withYear
      ? { month: 'short', day: 'numeric', year: 'numeric' }
      : { month: 'short', day: 'numeric' },
  )
}

export function range(start: string, end: string): string {
  const s = isoDate(start)
  const e = isoDate(end)
  return s === e ? fmt(s) : `${fmt(s, false)} – ${fmt(e)}`
}

/** Weekday-only count for the in-modal preview; the API returns the authoritative
 *  figure (which also excludes Serbian public holidays) on the created request. */
export function estimateWorkingDays(start: string, end: string): number {
  let d = new Date(`${start}T00:00`)
  const last = new Date(`${end}T00:00`)
  let n = 0
  while (d <= last) {
    const day = d.getDay()
    if (day !== 0 && day !== 6) n++
    d = new Date(d.getTime() + 86400000)
  }
  return n
}

export function workingDaysNoun(n: number): string {
  return n === 1 ? 'working day' : 'working days'
}

export function greeting(): string {
  const h = new Date().getHours()
  return h < 12 ? 'Good morning' : h < 18 ? 'Good afternoon' : 'Good evening'
}

export function todayLong(): string {
  return new Date().toLocaleDateString(LOCALE, {
    weekday: 'long',
    month: 'long',
    day: 'numeric',
    year: 'numeric',
  })
}
