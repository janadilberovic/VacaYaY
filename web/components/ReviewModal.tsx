'use client'

import { useState } from 'react'
import { useToast } from '@/state/toast'
import { leaveRequests } from '@/lib/endpoints'
import { ApiError } from '@/lib/api'
import type { LeaveRequestDto } from '@/lib/types'

interface Props {
  req: LeaveRequestDto
  action: 'approve' | 'reject'
  onClose: () => void
  onReviewed: (updated: LeaveRequestDto) => void
}

export function ReviewModal({ req, action, onClose, onReviewed }: Props) {
  const { toast } = useToast()
  const [comment, setComment] = useState('')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')

  const approving = action === 'approve'
  const title = approving ? 'Approve request' : 'Reject request'
  const desc = approving
    ? 'The employee will be notified and the days deducted where applicable.'
    : 'The employee will be notified with your comment.'

  async function confirm() {
    setBusy(true)
    setError('')
    try {
      const note = comment.trim() || null
      const updated = approving ? await leaveRequests.approve(req.id, note) : await leaveRequests.reject(req.id, note)
      toast(approving ? 'Request approved.' : 'Request rejected.')
      onReviewed(updated)
      onClose()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Something went wrong.')
      setBusy(false)
    }
  }

  return (
    <div className="modal-backdrop" style={{ alignItems: 'center', padding: 20, zIndex: 60 }}>
      <div className="modal-card" style={{ width: 420, padding: '22px 24px' }}>
        <div style={{ fontSize: 15.5, fontWeight: 700, marginBottom: 6 }}>{title}</div>
        <div style={{ color: 'var(--text2)', fontSize: 13.5, marginBottom: 16 }}>{desc}</div>
        <label htmlFor="rv-comment" className="field-label">
          HR comment <span style={{ color: 'var(--text3)', fontWeight: 500 }}>(optional)</span>
        </label>
        <textarea
          id="rv-comment"
          rows={3}
          className="textarea"
          value={comment}
          onChange={(e) => setComment(e.target.value)}
          placeholder="Optional note for the employee…"
        />
        {error && <div className="err" style={{ marginTop: 8 }}>{error}</div>}
        <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 10, marginTop: 18 }}>
          <button
            className="btn"
            style={{ background: 'none', border: '1px solid var(--border)', color: 'var(--text2)', padding: '8px 14px', fontSize: 13 }}
            onClick={onClose}
          >
            Cancel
          </button>
          <button
            className="btn"
            style={{ background: approving ? 'var(--accent)' : 'var(--pill-rejected-fg)', color: '#fff', padding: '8px 16px', fontSize: 13 }}
            disabled={busy}
            onClick={confirm}
          >
            {approving ? 'Approve' : 'Reject'}
          </button>
        </div>
      </div>
    </div>
  )
}
