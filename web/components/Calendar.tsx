'use client'

import { useState } from 'react'
import { pad, todayISO } from '@/lib/dates'

interface Props {
  start: string
  end: string
  onSelectDay: (iso: string) => void
}

const LOCALE = 'en-US'

/** Range picker: click a start day, then an end day. Past days are disabled. */
export function Calendar({ start, end, onSelectDay }: Props) {
  const base = new Date(`${start || todayISO()}T00:00`)
  const [view, setView] = useState({ y: base.getFullYear(), m: base.getMonth() })
  const today = todayISO()

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
  for (let i = 0; i < lead; i++) cells.push(<div key={`lead-${i}`} style={{ height: 26 }} />)

  for (let d = 1; d <= dim; d++) {
    const iso = `${view.y}-${pad(view.m + 1)}-${pad(d)}`
    const dow = new Date(view.y, view.m, d).getDay()
    const past = iso < today
    const isEdge = iso === start || iso === end
    const inRange = !!start && !!end && iso > start && iso < end
    cells.push(
      <button
        key={iso}
        onClick={() => !past && onSelectDay(iso)}
        style={{
          height: 26,
          border: 'none',
          borderRadius: inRange ? 0 : 8,
          background: isEdge
            ? 'var(--accent)'
            : inRange
            ? 'color-mix(in oklab, var(--accent) 14%, transparent)'
            : 'transparent',
          color: isEdge ? '#fff' : past || dow === 0 || dow === 6 ? 'var(--text3)' : 'var(--text)',
          fontSize: 11.5,
          fontWeight: isEdge ? 700 : 500,
          cursor: past ? 'default' : 'pointer',
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
      <div style={{ fontSize: 11, color: 'var(--text3)', marginTop: 8 }}>
        Click a start day, then an end day to select the range.
      </div>
    </div>
  )
}
