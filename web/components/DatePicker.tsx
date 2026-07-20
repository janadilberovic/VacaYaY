'use client'

import { useEffect, useRef, useState } from 'react'
import { fmt, isoDate, pad, todayISO } from '@/lib/dates'

interface Props {
  value: string
  onChange: (iso: string) => void
  id?: string
  placeholder?: string
}

const LOCALE = 'en-US'

/** Single-date popover picker styled to match the app. Any day is selectable
 *  (a hire date may be in the past), with Clear and Today shortcuts. */
export function DatePicker({ value, onChange, id, placeholder = 'Select a date' }: Props) {
  const [open, setOpen] = useState(false)
  const wrap = useRef<HTMLDivElement>(null)

  const selected = value ? isoDate(value) : ''
  const today = todayISO()
  const base = new Date(`${selected || today}T00:00`)
  const [view, setView] = useState({ y: base.getFullYear(), m: base.getMonth() })

  useEffect(() => {
    if (!open) return
    setView({ y: base.getFullYear(), m: base.getMonth() })
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open])

  useEffect(() => {
    if (!open) return
    function onDoc(e: MouseEvent) {
      if (wrap.current && !wrap.current.contains(e.target as Node)) setOpen(false)
    }
    function onKey(e: KeyboardEvent) {
      if (e.key === 'Escape') setOpen(false)
    }
    document.addEventListener('mousedown', onDoc)
    document.addEventListener('keydown', onKey)
    return () => {
      document.removeEventListener('mousedown', onDoc)
      document.removeEventListener('keydown', onKey)
    }
  }, [open])

  function shift(delta: number) {
    setView((v) => {
      let m = v.m + delta
      let y = v.y
      if (m < 0) { m = 11; y-- }
      if (m > 11) { m = 0; y++ }
      return { y, m }
    })
  }

  function pick(iso: string) {
    onChange(iso)
    setOpen(false)
  }

  const first = new Date(view.y, view.m, 1)
  const lead = (first.getDay() + 6) % 7
  const dim = new Date(view.y, view.m + 1, 0).getDate()

  const dows: string[] = []
  for (let i = 0; i < 7; i++) {
    dows.push(new Date(2026, 0, 5 + i).toLocaleDateString(LOCALE, { weekday: 'short' }).replace('.', ''))
  }

  const cells: React.ReactNode[] = []
  for (let i = 0; i < lead; i++) cells.push(<div key={`lead-${i}`} style={{ height: 28 }} />)
  for (let d = 1; d <= dim; d++) {
    const iso = `${view.y}-${pad(view.m + 1)}-${pad(d)}`
    const isSel = iso === selected
    const isToday = iso === today
    cells.push(
      <button
        key={iso}
        type="button"
        onClick={() => pick(iso)}
        style={{
          height: 28,
          border: isToday && !isSel ? '1px solid var(--accent)' : '1px solid transparent',
          borderRadius: 8,
          background: isSel ? 'var(--accent)' : 'transparent',
          color: isSel ? '#fff' : 'var(--text)',
          fontSize: 12,
          fontWeight: isSel ? 700 : 500,
          cursor: 'pointer',
          fontFamily: 'inherit',
          padding: 0,
        }}
      >
        {d}
      </button>,
    )
  }

  const navBtn: React.CSSProperties = {
    width: 24,
    height: 24,
    border: '1px solid var(--border)',
    borderRadius: 6,
    background: 'var(--surface)',
    color: 'var(--text2)',
    cursor: 'pointer',
    fontSize: 12,
    padding: 0,
  }

  const link: React.CSSProperties = {
    background: 'none',
    border: 'none',
    color: 'var(--accent)',
    fontSize: 12,
    fontWeight: 600,
    cursor: 'pointer',
    padding: 0,
    fontFamily: 'inherit',
  }

  return (
    <div ref={wrap} style={{ position: 'relative' }}>
      <button
        id={id}
        type="button"
        className="input"
        onClick={() => setOpen((o) => !o)}
        style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          gap: 8,
          cursor: 'pointer',
          textAlign: 'left',
          color: selected ? 'var(--text)' : 'var(--text3)',
        }}
      >
        <span>{selected ? fmt(selected) : placeholder}</span>
        <span aria-hidden style={{ color: 'var(--text3)', fontSize: 13 }}>▾</span>
      </button>

      {open && (
        <div
          style={{
            position: 'absolute',
            top: 'calc(100% + 6px)',
            left: 0,
            zIndex: 60,
            width: 268,
            maxWidth: '90vw',
            border: '1px solid var(--border)',
            borderRadius: 12,
            padding: 12,
            background: 'var(--surface)',
            boxShadow: 'var(--shadow)',
          }}
        >
          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 8 }}>
            <button type="button" style={navBtn} onClick={() => shift(-1)}>‹</button>
            <div style={{ fontSize: 12.5, fontWeight: 650, textTransform: 'capitalize' }}>
              {first.toLocaleDateString(LOCALE, { month: 'long', year: 'numeric' })}
            </div>
            <button type="button" style={navBtn} onClick={() => shift(1)}>›</button>
          </div>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(7,1fr)', marginBottom: 2 }}>
            {dows.map((w) => (
              <div
                key={w}
                style={{
                  textAlign: 'center',
                  fontSize: 9.5,
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
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(7,1fr)', gap: 1 }}>{cells}</div>
          <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: 10 }}>
            <button type="button" style={link} onClick={() => pick('')}>Clear</button>
            <button type="button" style={link} onClick={() => pick(today)}>Today</button>
          </div>
        </div>
      )}
    </div>
  )
}
