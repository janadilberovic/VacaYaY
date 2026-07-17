'use client'

import { useState } from 'react'
import { useAllRequests } from '@/components/useAllRequests'
import { ReviewModal } from '@/components/ReviewModal'
import { HrRequestDetail } from '@/components/HrRequestDetail'
import { StatusPill } from '@/components/Pill'
import { Avatar, EmptyState, ListSkeleton } from '@/components/ui'
import { colorHex, leaveTypeLabel } from '@/lib/leave'
import { range } from '@/lib/dates'
import { initialsFromName } from '@/lib/format'
import type { LeaveRequestStatus, LeaveRequestDto } from '@/lib/types'

const FILTERS: Array<{ key: 'all' | LeaveRequestStatus; label: string }> = [
  { key: 'all', label: 'All' },
  { key: 'Pending', label: 'Pending' },
  { key: 'Approved', label: 'Approved' },
  { key: 'Rejected', label: 'Rejected' },
]

export default function HrRequestsPage() {
  const { requests, typeMap, loading, patch } = useAllRequests()
  const [filter, setFilter] = useState<'all' | LeaveRequestStatus>('all')
  const [detail, setDetail] = useState<LeaveRequestDto | null>(null)
  const [review, setReview] = useState<{ req: LeaveRequestDto; action: 'approve' | 'reject' } | null>(null)

  const sorted = [...requests].sort((a, b) => {
    const ap = a.status === 'Pending'
    const bp = b.status === 'Pending'
    if (ap !== bp) return ap ? -1 : 1
    return a.startDate < b.startDate ? 1 : -1
  })
  const rows = sorted.filter((r) => filter === 'all' || r.status === filter)

  function onReviewed(updated: LeaveRequestDto) {
    patch(updated)
    setDetail(null)
  }

  return (
    <div style={{ animation: 'fade .25s' }}>
      <div className="page-head">
        <div className="page-h">Requests</div>
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
        <ListSkeleton rows={5} />
      ) : rows.length === 0 ? (
        <EmptyState title="No requests" desc="Try a different filter." />
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
          {rows.map((r) => {
            const type = typeMap.get(r.leaveTypeId)
            return (
              <div
                key={r.id}
                onClick={() => setDetail(r)}
                style={{
                  background: 'var(--surface)',
                  border: '1px solid var(--border)',
                  borderRadius: 12,
                  padding: '13px 16px',
                  animation: 'fade .3s',
                  cursor: 'pointer',
                }}
              >
                <div
                  style={{
                    display: 'grid',
                    gridTemplateColumns: 'minmax(140px,1fr) 104px 168px 40px 88px 66px',
                    alignItems: 'center',
                    gap: 12,
                    fontSize: 13.5,
                  }}
                >
                  <div style={{ display: 'flex', alignItems: 'center', gap: 9, minWidth: 0 }}>
                    <Avatar text={initialsFromName(r.employeeName)} />
                    <span style={{ fontWeight: 600, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                      {r.employeeName}
                    </span>
                  </div>
                  <span style={{ display: 'flex', alignItems: 'center', gap: 7, fontWeight: 600, whiteSpace: 'nowrap' }}>
                    <span style={{ width: 8, height: 8, borderRadius: '50%', background: colorHex(type?.color), flexShrink: 0 }} />
                    {leaveTypeLabel(r.leaveTypeName)}
                  </span>
                  <span style={{ color: 'var(--text2)', whiteSpace: 'nowrap' }}>{range(r.startDate, r.endDate)}</span>
                  <span style={{ color: 'var(--text3)', fontSize: 12.5, whiteSpace: 'nowrap' }}>{r.workingDays}d</span>
                  <span style={{ justifySelf: 'start' }}>
                    <StatusPill status={r.status} />
                  </span>
                  <div style={{ display: 'flex', gap: 6, justifyContent: 'flex-start' }}>
                    {r.status === 'Pending' && (
                      <>
                        <button
                          title="Approve"
                          onClick={(e) => { e.stopPropagation(); setReview({ req: r, action: 'approve' }) }}
                          style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'var(--pill-approved-bg)', color: 'var(--pill-approved-fg)', border: 'none', borderRadius: 8, width: 32, height: 30, fontSize: 15, fontWeight: 700, cursor: 'pointer' }}
                        >
                          ✓
                        </button>
                        <button
                          title="Reject"
                          onClick={(e) => { e.stopPropagation(); setReview({ req: r, action: 'reject' }) }}
                          style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'none', border: '1px solid var(--border)', color: 'var(--pill-rejected-fg)', borderRadius: 8, width: 32, height: 30, fontSize: 15, fontWeight: 700, cursor: 'pointer' }}
                        >
                          ✕
                        </button>
                      </>
                    )}
                  </div>
                </div>
                {r.reviewedAt && (r.hrComment || r.hrName) && (
                  <div style={{ color: 'var(--text3)', fontSize: 12.5, marginTop: 8, paddingLeft: 35 }}>
                    {r.hrComment ? `“${r.hrComment}” — ${r.hrName}` : r.hrName}
                  </div>
                )}
              </div>
            )
          })}
        </div>
      )}

      {detail && (
        <HrRequestDetail
          req={detail}
          type={typeMap.get(detail.leaveTypeId)}
          onClose={() => setDetail(null)}
          onReview={(action) => { setReview({ req: detail, action }); setDetail(null) }}
        />
      )}
      {review && (
        <ReviewModal
          req={review.req}
          action={review.action}
          onClose={() => setReview(null)}
          onReviewed={onReviewed}
        />
      )}
    </div>
  )
}
