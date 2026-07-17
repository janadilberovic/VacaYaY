'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { StatusPill } from './Pill'
import { ListSkeleton, EmptyState } from './ui'
import { useLeaveModal } from '@/state/leaveModal'
import { colorHex, leaveTypeLabel } from '@/lib/leave'
import { range, workingDaysNoun } from '@/lib/dates'
import type { LeaveRequestStatus, LeaveRequestDto, LeaveTypeDto } from '@/lib/types'

const FILTERS: Array<{ key: 'all' | LeaveRequestStatus; label: string }> = [
  { key: 'all', label: 'All' },
  { key: 'Pending', label: 'Pending' },
  { key: 'Approved', label: 'Approved' },
  { key: 'Rejected', label: 'Rejected' },
  { key: 'Cancelled', label: 'Cancelled' },
]

interface Props {
  requests: LeaveRequestDto[]
  typeMap: Map<number, LeaveTypeDto>
  loading: boolean
  heading?: string
}

export function RequestList({ requests, typeMap, loading, heading }: Props) {
  const router = useRouter()
  const { open } = useLeaveModal()
  const [filter, setFilter] = useState<'all' | LeaveRequestStatus>('all')

  const sorted = [...requests].sort((a, b) => (a.createdAt < b.createdAt ? 1 : -1))
  const rows = sorted.filter((r) => filter === 'all' || r.status === filter)

  return (
    <div>
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 12, marginBottom: 14 }}>
        {heading ? <div style={{ fontSize: 15, fontWeight: 650 }}>{heading}</div> : <div />}
        <div style={{ display: 'flex', gap: 6, flexWrap: 'wrap' }}>
          {FILTERS.map((f) => {
            const on = filter === f.key
            return (
              <button
                key={f.key}
                onClick={() => setFilter(f.key)}
                style={{
                  border: `1px solid ${on ? 'var(--text)' : 'var(--border)'}`,
                  background: on ? 'var(--text)' : 'var(--surface)',
                  color: on ? 'var(--bg)' : 'var(--text2)',
                  borderRadius: 99,
                  padding: '5px 12px',
                  fontSize: 12,
                  fontWeight: 600,
                  cursor: 'pointer',
                }}
              >
                {f.label}
              </button>
            )
          })}
        </div>
      </div>

      {loading ? (
        <ListSkeleton />
      ) : rows.length === 0 ? (
        <EmptyState
          title={filter === 'all' ? 'No requests yet' : `No ${filter.toLowerCase()} requests`}
          desc={filter === 'all' ? 'Time off starts here — submit your first request.' : 'Try a different filter, or submit a new request.'}
          action={
            <button className="btn btn-ghost" style={{ padding: '8px 14px', fontSize: 13 }} onClick={open}>
              Request leave
            </button>
          }
        />
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 8, animation: 'fade .3s' }}>
          {rows.map((r) => {
            const type = typeMap.get(r.leaveTypeId)
            return (
              <button
                key={r.id}
                onClick={() => router.push(`/requests/${r.id}`)}
                style={{
                  display: 'grid',
                  gridTemplateColumns: '96px 140px 1fr auto 14px',
                  alignItems: 'center',
                  gap: 14,
                  padding: '14px 16px',
                  background: 'var(--surface)',
                  border: '1px solid var(--border)',
                  borderRadius: 12,
                  cursor: 'pointer',
                  textAlign: 'left',
                  fontSize: 13.5,
                  color: 'var(--text)',
                }}
              >
                <span style={{ justifySelf: 'start' }}>
                  <StatusPill status={r.status} />
                </span>
                <span style={{ display: 'flex', alignItems: 'center', gap: 7, fontWeight: 600 }}>
                  <span style={{ width: 8, height: 8, borderRadius: '50%', background: colorHex(type?.color), flexShrink: 0 }} />
                  {leaveTypeLabel(r.leaveTypeName)}
                </span>
                <span style={{ color: 'var(--text2)' }}>{range(r.startDate, r.endDate)}</span>
                <span style={{ color: 'var(--text3)', fontSize: 12.5 }}>
                  {r.workingDays} {workingDaysNoun(r.workingDays)}
                </span>
                <span style={{ color: 'var(--text3)' }}>›</span>
              </button>
            )
          })}
        </div>
      )}
    </div>
  )
}
