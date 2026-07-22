'use client'

import { useEffect, useState } from 'react'
import { useAuth } from '@/state/auth'
import { useLeaveModal } from '@/state/leaveModal'
import { useMyRequests } from '@/components/useMyRequests'
import { RequestList } from '@/components/RequestList'
import { leaveRequests } from '@/lib/endpoints'
import { greeting, isoDate, todayLong } from '@/lib/dates'
import type { LeaveBalance } from '@/lib/types'

export default function DashboardPage() {
  const { user } = useAuth()
  const { open } = useLeaveModal()
  const { requests, typeMap, loading } = useMyRequests()
  const [balance, setBalance] = useState<LeaveBalance | null>(null)

  useEffect(() => {
    leaveRequests
      .balance()
      .then(setBalance)
      .catch(() => setBalance(null))
  }, [])

  const year = new Date().getFullYear()
  const counts = (id: number) => typeMap.get(id)?.countsAgainstBalance ?? false

  // Days consumed this calendar year have no API aggregate — the balance endpoint is year-agnostic.
  const usedDays = requests
    .filter(
      (r) =>
        r.status === 'Approved' &&
        counts(r.leaveTypeId) &&
        new Date(isoDate(r.startDate)).getFullYear() === year,
    )
    .reduce((sum, r) => sum + r.workingDays, 0)

  const remaining = balance?.remainingDays ?? 0
  const pendingDays = balance?.pendingDays ?? 0
  const allowance = Math.max(1, remaining + pendingDays + usedDays)
  const usedPct = `${Math.round((usedDays / allowance) * 100)}%`
  const pendingPct = `${Math.round((pendingDays / allowance) * 100)}%`

  return (
    <>
      <div className="page-head">
        <div>
          <div style={{ fontSize: 22, fontWeight: 700, letterSpacing: '-0.02em' }}>
            {greeting()}, {user?.firstName}
          </div>
          <div style={{ color: 'var(--text3)', fontSize: 13, marginTop: 2 }}>{todayLong()}</div>
        </div>
        <button className="btn btn-primary" onClick={open}>
          ＋ Request leave
        </button>
      </div>

      <div className="card" style={{ padding: 24, marginBottom: 32 }}>
        <div className="section-label" style={{ fontSize: 12, letterSpacing: '.07em' }}>
          Leave balance
        </div>
        <div style={{ display: 'flex', alignItems: 'baseline', gap: 10, marginTop: 6 }}>
          <div style={{ fontSize: 44, fontWeight: 700, letterSpacing: '-0.03em', lineHeight: 1 }}>
            {remaining}
          </div>
          <div style={{ fontSize: 15, color: 'var(--text2)', fontWeight: 500 }}>days remaining</div>
        </div>
        <div
          style={{
            display: 'flex',
            height: 8,
            borderRadius: 4,
            overflow: 'hidden',
            background: 'var(--surface2)',
            marginTop: 16,
          }}
        >
          <div style={{ width: usedPct, background: 'var(--accent)' }} />
          <div style={{ width: pendingPct, background: 'var(--accent)', opacity: 0.35 }} />
        </div>
        <div
          style={{ display: 'flex', gap: 18, marginTop: 10, fontSize: 12.5, color: 'var(--text2)' }}
        >
          <span>
            <span
              style={{
                display: 'inline-block',
                width: 8,
                height: 8,
                borderRadius: 2,
                background: 'var(--accent)',
                marginRight: 6,
              }}
            />
            {usedDays} used this year
          </span>
          <span>
            <span
              style={{
                display: 'inline-block',
                width: 8,
                height: 8,
                borderRadius: 2,
                background: 'var(--accent)',
                opacity: 0.35,
                marginRight: 6,
              }}
            />
            {pendingDays} pending approval
          </span>
        </div>
      </div>

      <RequestList requests={requests} typeMap={typeMap} loading={loading} heading="My requests" />
    </>
  )
}
