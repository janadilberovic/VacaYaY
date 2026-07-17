'use client'

import type { LeaveRequestStatus } from '@/lib/types'
import { STATUS_LABEL, pillKey } from '@/lib/leave'

export function StatusPill({ status }: { status: LeaveRequestStatus }) {
  const key = pillKey(status)
  return (
    <span
      className="pill"
      style={{ background: `var(--pill-${key}-bg)`, color: `var(--pill-${key}-fg)` }}
    >
      {STATUS_LABEL[status]}
    </span>
  )
}

export function Badge({ children }: { children: React.ReactNode }) {
  return (
    <span
      style={{
        border: '1px solid var(--border)',
        borderRadius: 6,
        padding: '2px 8px',
        fontSize: 11,
        fontWeight: 600,
        color: 'var(--text2)',
      }}
    >
      {children}
    </span>
  )
}

export function Dot({ color }: { color: string }) {
  return (
    <span style={{ width: 9, height: 9, borderRadius: '50%', background: color, flexShrink: 0 }} />
  )
}
