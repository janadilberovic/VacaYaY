'use client'

import { useEffect, useState } from 'react'
import { useParams, useRouter } from 'next/navigation'
import { StatusPill, Badge } from '@/components/Pill'
import { ConfirmDialog } from '@/components/ConfirmDialog'
import { leaveRequests, leaveTypes as leaveTypesApi } from '@/lib/endpoints'
import { useToast } from '@/state/toast'
import { colorHex, leaveTypeLabel } from '@/lib/leave'
import { fmt, range, workingDaysNoun } from '@/lib/dates'
import type { LeaveRequestDto, LeaveTypeDto } from '@/lib/types'

export default function RequestDetailPage() {
  const params = useParams<{ id: string }>()
  const id = Number(params.id)
  const router = useRouter()
  const { toast } = useToast()

  const [req, setReq] = useState<LeaveRequestDto | null>(null)
  const [type, setType] = useState<LeaveTypeDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [confirm, setConfirm] = useState(false)

  useEffect(() => {
    let active = true
    setLoading(true)
    leaveRequests
      .byId(id)
      .then(async (r) => {
        if (!active) return
        setReq(r)
        const types = await leaveTypesApi.all().catch(() => [])
        if (active) setType(types.find((t) => t.id === r.leaveTypeId) ?? null)
      })
      .catch(() => active && setReq(null))
      .finally(() => active && setLoading(false))
    return () => {
      active = false
    }
  }, [id])

  async function cancel() {
    setConfirm(false)
    try {
      await leaveRequests.cancel(id)
      toast('Request cancelled.')
      router.push('/requests')
    } catch {
      toast('Could not cancel the request.', 'error')
    }
  }

  if (loading) return <div style={{ color: 'var(--text3)' }}>Loading…</div>
  if (!req) return <div style={{ color: 'var(--text3)' }}>Request not found.</div>

  return (
    <div style={{ animation: 'fade .25s' }}>
      <button
        onClick={() => router.push('/requests')}
        style={{ background: 'none', border: 'none', color: 'var(--text3)', fontSize: 13, fontWeight: 600, cursor: 'pointer', padding: 0, marginBottom: 20 }}
      >
        ← Back to requests
      </button>
      <div className="card" style={{ overflow: 'hidden' }}>
        <div style={{ padding: '24px 26px', borderBottom: '1px solid var(--border)' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 10, flexWrap: 'wrap' }}>
            <span style={{ display: 'flex', alignItems: 'center', gap: 7, fontWeight: 650, fontSize: 14 }}>
              <span style={{ width: 9, height: 9, borderRadius: '50%', background: colorHex(type?.color) }} />
              {leaveTypeLabel(req.leaveTypeName)}
            </span>
            <StatusPill status={req.status} />
            {type && <Badge>{type.isPaid ? 'Paid' : 'Unpaid'}</Badge>}
            {type?.countsAgainstBalance && <Badge>Counts against balance</Badge>}
          </div>
          <div style={{ fontSize: 23, fontWeight: 700, letterSpacing: '-0.02em' }}>{range(req.startDate, req.endDate)}</div>
          <div style={{ color: 'var(--text3)', fontSize: 13, marginTop: 4 }}>
            {req.workingDays} {workingDaysNoun(req.workingDays)} · requested {fmt(req.createdAt)}
          </div>
        </div>
        <div style={{ padding: '22px 26px', display: 'flex', flexDirection: 'column', gap: 18 }}>
          <div>
            <div className="section-label" style={{ marginBottom: 5 }}>Reason</div>
            <div style={{ color: 'var(--text2)' }}>{req.reason || 'No reason provided.'}</div>
          </div>
          {req.reviewedAt && (
            <div style={{ background: 'var(--surface2)', borderRadius: 10, padding: '14px 16px' }}>
              <div className="section-label" style={{ marginBottom: 6 }}>
                Reviewed by {req.hrName} · {fmt(req.reviewedAt)}
              </div>
              {req.hrComment && <div style={{ color: 'var(--text)' }}>“{req.hrComment}”</div>}
            </div>
          )}
          {req.status === 'Pending' && (
            <div style={{ display: 'flex', justifyContent: 'flex-end' }}>
              <button
                onClick={() => setConfirm(true)}
                className="btn"
                style={{ background: 'none', border: '1px solid var(--border)', color: 'var(--pill-rejected-fg)', padding: '9px 16px', fontSize: 13 }}
              >
                Cancel request
              </button>
            </div>
          )}
        </div>
      </div>

      {confirm && (
        <ConfirmDialog
          spec={{
            title: 'Cancel this request?',
            message: 'Your pending request will be withdrawn. HR won’t review it, and you can submit a new one anytime.',
            confirmLabel: 'Cancel request',
            onConfirm: cancel,
          }}
          onCancel={() => setConfirm(false)}
        />
      )}
    </div>
  )
}
