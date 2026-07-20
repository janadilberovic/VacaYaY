'use client'

import { Modal } from './Modal'
import { Badge } from './Pill'
import { Avatar } from './ui'
import { colorHex, leaveTypeLabel, pillKey, STATUS_LABEL } from '@/lib/leave'
import { fmt, range, workingDaysNoun } from '@/lib/dates'
import { initialsFromName } from '@/lib/format'
import type { LeaveRequestDto, LeaveTypeDto } from '@/lib/types'

interface Props {
  req: LeaveRequestDto
  type: LeaveTypeDto | undefined
  onClose: () => void
  onReview: (action: 'approve' | 'reject') => void
}

export function HrRequestDetail({ req, type, onClose, onReview }: Props) {
  return (
    <Modal onClose={onClose} width={520}>
      <div style={{ padding: '22px 24px', borderBottom: '1px solid var(--border)' }}>
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 14 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <Avatar text={initialsFromName(req.employeeName)} size={32} />
            <div>
              <div style={{ fontWeight: 650, fontSize: 14 }}>{req.employeeName}</div>
            </div>
          </div>
          <button className="modal-x" onClick={onClose}>✕</button>
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: 10, flexWrap: 'wrap' }}>
          <span style={{ display: 'flex', alignItems: 'center', gap: 7, fontWeight: 650, fontSize: 14 }}>
            <span style={{ width: 9, height: 9, borderRadius: '50%', background: colorHex(type?.color) }} />
            {leaveTypeLabel(req.leaveTypeName)}
          </span>
          <span className="pill" style={{ background: `var(--pill-${pillKey(req.status)}-bg)`, color: `var(--pill-${pillKey(req.status)}-fg)` }}>
            {STATUS_LABEL[req.status]}
          </span>
          {type && <Badge>{type.isPaid ? 'Paid' : 'Unpaid'}</Badge>}
          {type?.countsAgainstBalance && <Badge>Counts against balance</Badge>}
        </div>
      </div>
      <div style={{ padding: '20px 24px', display: 'flex', flexDirection: 'column', gap: 16 }}>
        <div>
          <div style={{ fontSize: 20, fontWeight: 700, letterSpacing: '-0.02em' }}>{range(req.startDate, req.endDate)}</div>
          <div style={{ color: 'var(--text3)', fontSize: 13, marginTop: 3 }}>
            {req.workingDays} {workingDaysNoun(req.workingDays)} · requested {fmt(req.createdAt)}
          </div>
        </div>
        <div>
          <div className="section-label" style={{ marginBottom: 5 }}>Reason</div>
          <div style={{ color: 'var(--text2)' }}>{req.reason || 'No reason provided.'}</div>
        </div>
        {req.reviewedAt && (
          <div style={{ background: 'var(--surface2)', borderRadius: 10, padding: '14px 16px' }}>
            <div className="section-label" style={{ marginBottom: 6 }}>
              Reviewed · {req.hrName} · {fmt(req.reviewedAt)}
            </div>
            {req.hrComment && <div style={{ color: 'var(--text)' }}>“{req.hrComment}”</div>}
          </div>
        )}
      </div>
      {req.status === 'Pending' && (
        <div className="modal-foot">
          <button
            className="btn"
            style={{ background: 'none', border: '1px solid var(--border)', color: 'var(--pill-rejected-fg)', padding: '9px 18px', fontSize: 13 }}
            onClick={() => onReview('reject')}
          >
            Reject
          </button>
          <button
            className="btn"
            style={{ background: 'var(--pill-approved-fg)', color: '#fff', border: 'none', padding: '9px 18px', fontSize: 13 }}
            onClick={() => onReview('approve')}
          >
            Approve
          </button>
        </div>
      )}
    </Modal>
  )
}
