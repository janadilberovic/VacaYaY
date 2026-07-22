'use client'

import { useEffect, useMemo, useState } from 'react'
import { useAllRequests } from '@/components/useAllRequests'
import { ReviewModal } from '@/components/ReviewModal'
import { HrRequestDetail } from '@/components/HrRequestDetail'
import { Avatar, EmptyState } from '@/components/ui'
import { employees as employeesApi, leaveRequests } from '@/lib/endpoints'
import { colorHex, leaveTypeLabel } from '@/lib/leave'
import { range, workingDaysNoun } from '@/lib/dates'
import { initialsFromName } from '@/lib/format'
import type { LeaveRequestDto, LeaveRequestSummary } from '@/lib/types'

function StatCard({
  label,
  value,
  color,
}: {
  label: string
  value: number | string
  color?: string
}) {
  return (
    <div className="card" style={{ padding: 20 }}>
      <div className="section-label" style={{ fontSize: 11.5, letterSpacing: '.07em' }}>
        {label}
      </div>
      <div
        style={{
          fontSize: 34,
          fontWeight: 700,
          letterSpacing: '-0.03em',
          marginTop: 6,
          color: color ?? 'var(--text)',
        }}
      >
        {value}
      </div>
    </div>
  )
}

function Bar({
  label,
  dot,
  value,
  pct,
  color,
}: {
  label: string
  dot?: string
  value: string
  pct: string
  color: string
}) {
  return (
    <div>
      <div
        style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          fontSize: 12.5,
          marginBottom: 5,
        }}
      >
        <span style={{ display: 'flex', alignItems: 'center', gap: 7, fontWeight: 600 }}>
          {dot && <span style={{ width: 8, height: 8, borderRadius: '50%', background: dot }} />}
          {label}
        </span>
        <span style={{ color: 'var(--text3)' }}>{value}</span>
      </div>
      <div
        style={{ height: 8, borderRadius: 4, background: 'var(--surface2)', overflow: 'hidden' }}
      >
        <div
          style={{
            height: '100%',
            width: pct,
            background: color,
            borderRadius: 4,
            transition: 'width .4s',
          }}
        />
      </div>
    </div>
  )
}

const PENDING_PREVIEW = 8

// The charts cover every request, so they come from the summary endpoint rather than
// the paged list below them.
const PENDING_QUERY = {
  status: 'Pending' as const,
  pageSize: PENDING_PREVIEW,
  sortBy: 'StartDate' as const,
  sortDescending: false,
}

export default function HrDashboardPage() {
  const { requests: pending, typeMap, loading, patch } = useAllRequests(PENDING_QUERY)
  const [counts, setCounts] = useState({ headcount: 0, archived: 0 })
  const [summary, setSummary] = useState<LeaveRequestSummary | null>(null)
  const [detail, setDetail] = useState<LeaveRequestDto | null>(null)
  const [review, setReview] = useState<{
    req: LeaveRequestDto
    action: 'approve' | 'reject'
  } | null>(null)

  useEffect(() => {
    // Only the totals are needed here — ask for one row and read the counts.
    Promise.all([
      employeesApi.list({ pageSize: 1 }),
      employeesApi.list({ pageSize: 1, archived: true }),
    ])
      .then(([active, arch]) =>
        setCounts({ headcount: active.totalCount, archived: arch.totalCount }),
      )
      .catch(() => setCounts({ headcount: 0, archived: 0 }))

    leaveRequests
      .summary()
      .then(setSummary)
      .catch(() => setSummary(null))
  }, [])

  const { headcount, archived } = counts
  const typeColors = useMemo(
    () => new Map([...typeMap.values()].map((t) => [t.id, t.color])),
    [typeMap],
  )

  const typeMax = Math.max(1, ...(summary?.daysByType.map((t) => t.workingDays) ?? []))
  const typeChart = (summary?.daysByType ?? []).map((t) => ({
    name: leaveTypeLabel(t.leaveTypeName),
    color: colorHex(typeColors.get(t.leaveTypeId)),
    days: String(t.workingDays),
    pct: `${Math.round((t.workingDays / typeMax) * 100)}%`,
  }))

  const statusColors: Record<string, string> = {
    Pending: 'var(--pill-pending-fg)',
    Approved: 'var(--pill-approved-fg)',
    Rejected: 'var(--pill-rejected-fg)',
  }
  const statusTotal = Math.max(1, summary?.totalCount ?? 0)
  const statusChart = (['Pending', 'Approved', 'Rejected'] as const).map((k) => {
    const count = summary?.countByStatus[k] ?? 0
    return {
      label: k,
      count: String(count),
      color: statusColors[k],
      pct: `${Math.round((count / statusTotal) * 100)}%`,
    }
  })

  return (
    <div style={{ animation: 'fade .25s' }}>
      <div className="page-h" style={{ marginBottom: 24 }}>
        HR dashboard
      </div>

      <div
        style={{ display: 'grid', gridTemplateColumns: 'repeat(3,1fr)', gap: 14, marginBottom: 32 }}
      >
        <StatCard
          label="Pending requests"
          value={summary?.countByStatus.Pending ?? 0}
          color="var(--pill-pending-fg)"
        />
        <StatCard label="Headcount" value={headcount} />
        <StatCard label="Archived accounts" value={archived} color="var(--text3)" />
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: '1.5fr 1fr', gap: 14, marginBottom: 32 }}>
        <div className="card" style={{ padding: 20 }}>
          <div style={{ fontSize: 12.5, fontWeight: 650, marginBottom: 16 }}>
            Leave days by type
          </div>
          {typeChart.length === 0 ? (
            <div style={{ color: 'var(--text3)', fontSize: 13 }}>No data yet</div>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
              {typeChart.map((c) => (
                <Bar
                  key={c.name}
                  label={c.name}
                  dot={c.color}
                  value={c.days}
                  pct={c.pct}
                  color={c.color}
                />
              ))}
            </div>
          )}
        </div>
        <div className="card" style={{ padding: 20 }}>
          <div style={{ fontSize: 12.5, fontWeight: 650, marginBottom: 16 }}>
            Requests by status
          </div>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
            {statusChart.map((c) => (
              <Bar key={c.label} label={c.label} value={c.count} pct={c.pct} color={c.color} />
            ))}
          </div>
        </div>
      </div>

      <div style={{ fontSize: 15, fontWeight: 650, marginBottom: 14 }}>
        Pending review
        {(summary?.countByStatus.Pending ?? 0) > PENDING_PREVIEW && (
          <span style={{ color: 'var(--text3)', fontSize: 12.5, fontWeight: 500, marginLeft: 8 }}>
            oldest {PENDING_PREVIEW} of {summary?.countByStatus.Pending}
          </span>
        )}
      </div>
      {loading ? null : pending.length === 0 ? (
        <EmptyState title="All caught up" desc="No requests waiting for review." icon="✓" />
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
          {pending.map((r) => {
            const type = typeMap.get(r.leaveTypeId)
            return (
              <div
                key={r.id}
                onClick={() => setDetail(r)}
                style={{
                  display: 'grid',
                  gridTemplateColumns: '1.4fr 130px 1fr auto auto',
                  alignItems: 'center',
                  gap: 14,
                  padding: '13px 16px',
                  background: 'var(--surface)',
                  border: '1px solid var(--border)',
                  borderRadius: 12,
                  fontSize: 13.5,
                  animation: 'fade .3s',
                  cursor: 'pointer',
                }}
              >
                <div style={{ display: 'flex', alignItems: 'center', gap: 9, minWidth: 0 }}>
                  <Avatar text={initialsFromName(r.employeeName)} />
                  <span
                    style={{
                      fontWeight: 600,
                      whiteSpace: 'nowrap',
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                    }}
                  >
                    {r.employeeName}
                  </span>
                </div>
                <span style={{ display: 'flex', alignItems: 'center', gap: 7, fontWeight: 600 }}>
                  <span
                    style={{
                      width: 8,
                      height: 8,
                      borderRadius: '50%',
                      background: colorHex(type?.color),
                      flexShrink: 0,
                    }}
                  />
                  {leaveTypeLabel(r.leaveTypeName)}
                </span>
                <span style={{ color: 'var(--text2)' }}>
                  {range(r.startDate, r.endDate)} ·{' '}
                  <span style={{ color: 'var(--text3)', fontSize: 12.5 }}>
                    {r.workingDays} {workingDaysNoun(r.workingDays)}
                  </span>
                </span>
                <button
                  onClick={(e) => {
                    e.stopPropagation()
                    setReview({ req: r, action: 'approve' })
                  }}
                  style={{
                    background: 'var(--pill-approved-bg)',
                    color: 'var(--pill-approved-fg)',
                    border: 'none',
                    borderRadius: 8,
                    padding: '7px 14px',
                    fontSize: 12,
                    fontWeight: 650,
                    cursor: 'pointer',
                  }}
                >
                  Approve
                </button>
                <button
                  onClick={(e) => {
                    e.stopPropagation()
                    setReview({ req: r, action: 'reject' })
                  }}
                  style={{
                    background: 'none',
                    border: '1px solid var(--border)',
                    color: 'var(--pill-rejected-fg)',
                    borderRadius: 8,
                    padding: '7px 14px',
                    fontSize: 12,
                    fontWeight: 650,
                    cursor: 'pointer',
                  }}
                >
                  Reject
                </button>
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
          onReview={(action) => {
            setReview({ req: detail, action })
            setDetail(null)
          }}
        />
      )}
      {review && (
        <ReviewModal
          req={review.req}
          action={review.action}
          onClose={() => setReview(null)}
          onReviewed={(u) => {
            patch(u)
            setDetail(null)
          }}
        />
      )}
    </div>
  )
}
