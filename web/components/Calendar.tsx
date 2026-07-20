'use client'

import { useState } from 'react'
import { isWeekendISO, pad, todayISO } from '@/lib/dates'
import { useHolidays } from '@/lib/holidays'

interface Props {
  start: string
  end: string
  onSelectDay: (iso: string) => void
  /** ISO dates already covered by a pending/approved request — shaded and not selectable. */
  bookedDays?: Set<string>
}

const LOCALE = 'en-US'

const LEGEND = [
  { swatch: 'var(--accent)', label: 'Selected' },
  { swatch: 'var(--pill-pending-bg)', label: 'Already requested' },
  { swatch: 'var(--pill-rejected-bg)', label: 'Holiday' },
  { swatch: 'var(--surface2)', label: 'Weekend' },
]

/** Range picker: click a start day, then an end day. Past and already-requested days are disabled. */
export function Calendar({ start, end, onSelectDay, bookedDays }: Props) {
  const base = new Date(`${start || todayISO()}T00:00`)
  const [view, setView] = useState({ y: base.getFullYear(), m: base.getMonth() })
  const today = todayISO()
  const holidays = useHolidays([view.y])

  function shift(delta: number) {
    setView((v) => {
      let m = v.m + delta
      let y = v.y
      if (m < 0) { m = 11; y-- }
      if (m > 11) { m = 0; y++ }
      return { y, m }
    })
  }

  const first = new Date(view.y, view.m, 1)
  const lead = (first.getDay() + 6) % 7
  const dim = new Date(view.y, view.m + 1, 0).getDate()

  const dows: string[] = []
  for (let i = 0; i < 7; i++) {
    dows.push(new Date(2026, 0, 5 + i).toLocaleDateString(LOCALE, { weekday: 'short' }).replace('.', ''))
  }

  const cells: React.ReactNode[] = []
  for (let i = 0; i < lead; i++) cells.push(<div key={`lead-${i}`} style={{ height: 30 }} />)

  for (let d = 1; d <= dim; d++) {
    const iso = `${view.y}-${pad(view.m + 1)}-${pad(d)}`
    const past = iso < today
    const isEdge = iso === start || iso === end
    const inRange = !!start && !!end && iso > start && iso < end
    const booked = !!bookedDays?.has(iso)
    const holiday = holidays.has(iso)
    const weekend = isWeekendISO(iso)
    const disabled = past || booked

    let background = 'transparent'
    let color = 'var(--text)'
    let title: string | undefined

    if (isEdge) {
      background = 'var(--accent)'
      color = '#fff'
    } else if (inRange) {
      background = 'color-mix(in oklab, var(--accent) 14%, transparent)'
    } else if (past) {
      color = 'var(--text3)'
    } else if (booked) {
      background = 'var(--pill-pending-bg)'
      color = 'var(--pill-pending-fg)'
      title = 'Already requested'
    } else if (holiday) {
      background = 'var(--pill-rejected-bg)'
      color = 'var(--pill-rejected-fg)'
      title = 'Public holiday'
    } else if (weekend) {
      background = 'var(--surface2)'
      color = 'var(--text3)'
      title = 'Weekend'
    }

    cells.push(
      <button
        key={iso}
        onClick={() => !disabled && onSelectDay(iso)}
        disabled={disabled}
        title={title}
        style={{
          height: 30,
          border: 'none',
          borderRadius: inRange ? 0 : 8,
          background,
          color,
          fontSize: 11.5,
          fontWeight: isEdge ? 700 : 500,
          textDecoration: booked && !isEdge ? 'line-through' : 'none',
          cursor: disabled ? 'default' : 'pointer',
          opacity: past ? 0.55 : 1,
          fontFamily: 'inherit',
          padding: 0,
        }}
      >
        {d}
      </button>,
    )
  }

  const navBtn: React.CSSProperties = {
    width: 22,
    height: 22,
    border: '1px solid var(--border)',
    borderRadius: 6,
    background: 'var(--surface)',
    color: 'var(--text2)',
    cursor: 'pointer',
    fontSize: 11,
    padding: 0,
  }

  return (
    <div style={{ border: '1px solid var(--border)', borderRadius: 10, padding: 12, background: 'var(--surface)' }}>
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 6 }}>
        <button style={navBtn} onClick={() => shift(-1)}>‹</button>
        <div style={{ fontSize: 12, fontWeight: 650, textTransform: 'capitalize' }}>
          {first.toLocaleDateString(LOCALE, { month: 'long', year: 'numeric' })}
        </div>
        <button style={navBtn} onClick={() => shift(1)}>›</button>
      </div>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(7,1fr)', marginBottom: 2 }}>
        {dows.map((w) => (
          <div
            key={w}
            style={{
              textAlign: 'center',
              fontSize: 9,
              fontWeight: 600,
              letterSpacing: '.03em',
              textTransform: 'uppercase',
              color: 'var(--text3)',
              padding: '2px 0',
            }}
          >
            {w}
          </div>
        ))}
      </div>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(7,1fr)' }}>{cells}</div>
      <div style={{ display: 'flex', flexWrap: 'wrap', gap: '5px 14px', marginTop: 10 }}>
        {LEGEND.map((l) => (
          <div key={l.label} style={{ display: 'flex', alignItems: 'center', gap: 5, fontSize: 10.5, color: 'var(--text3)' }}>
            <span style={{ width: 10, height: 10, borderRadius: 3, background: l.swatch, border: '1px solid var(--border)' }} />
            {l.label}
          </div>
        ))}
      </div>
      <div style={{ fontSize: 11, color: 'var(--text3)', marginTop: 8 }}>
        Click a start day, then an end day to select the range.
      </div>
    </div>
  )
}
