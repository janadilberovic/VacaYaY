'use client'

import { useEffect, useRef, useState } from 'react'

export interface SelectOption {
  value: string
  label: string
  disabled?: boolean
}

interface Props {
  value: string
  options: SelectOption[]
  onChange: (value: string) => void
  id?: string
  /** Accessible name for triggers with no visible <label> — e.g. the filter row. */
  label?: string
  /** Pill-sized trigger for filter rows; the default matches a form input. */
  compact?: boolean
}

const PANEL_WIDTH = 260

/** Themed replacement for a native select — a popover list styled like the rest of the app. */
export function Select({ value, options, onChange, id, label, compact }: Props) {
  const [open, setOpen] = useState(false)
  const [alignRight, setAlignRight] = useState(false)
  const wrap = useRef<HTMLDivElement>(null)

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

  function toggle() {
    // A trigger near the right edge would push the panel off-screen — anchor it right instead.
    const box = wrap.current?.getBoundingClientRect()
    if (box) setAlignRight(box.left + PANEL_WIDTH > window.innerWidth - 12)
    setOpen((o) => !o)
  }

  const selected = options.find((o) => o.value === value)

  return (
    <div ref={wrap} style={{ position: 'relative' }}>
      <button
        id={id}
        type="button"
        className={`input select-trigger${compact ? ' compact' : ''}`}
        // Select-only combobox: an associated <label> owns the name, so the trigger's
        // text content has to be exposed as the value rather than the name.
        role="combobox"
        aria-haspopup="listbox"
        aria-expanded={open}
        aria-label={label}
        onClick={toggle}
      >
        <span>{selected?.label ?? ''}</span>
        <span aria-hidden style={{ color: 'var(--text3)', fontSize: 13 }}>
          ▾
        </span>
      </button>

      {open && (
        <div role="listbox" className={`select-panel${alignRight ? ' right' : ''}`}>
          {options.map((o) => (
            <button
              key={o.value}
              type="button"
              role="option"
              aria-selected={o.value === value}
              disabled={o.disabled}
              className={`select-option${o.value === value ? ' on' : ''}`}
              onClick={() => {
                onChange(o.value)
                setOpen(false)
              }}
            >
              <span>{o.label}</span>
              {o.value === value && (
                <span aria-hidden style={{ color: 'var(--accent)' }}>
                  ✓
                </span>
              )}
            </button>
          ))}
        </div>
      )}
    </div>
  )
}
