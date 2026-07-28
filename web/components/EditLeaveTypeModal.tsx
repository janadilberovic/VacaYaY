'use client'

import { useState } from 'react'
import { Modal, ModalHeader } from './Modal'
import { Toggle } from './Toggle'
import { useToast } from '@/state/toast'
import { leaveTypes as leaveTypesApi } from '@/lib/endpoints'
import { ApiError } from '@/lib/api'
import { COLOR_HEX, COLOR_ORDER, leaveTypeLabel } from '@/lib/leave'
import type { LeaveColor, LeaveTypeDto } from '@/lib/types'

export function EditLeaveTypeModal({
  leaveType,
  onClose,
  onSaved,
}: {
  leaveType: LeaveTypeDto
  onClose: () => void
  onSaved: (t: LeaveTypeDto) => void
}) {
  const { toast } = useToast()
  const [color, setColor] = useState<LeaveColor | null>(leaveType.color)
  const [isPaid, setIsPaid] = useState(leaveType.isPaid)
  const [counts, setCounts] = useState(leaveType.countsAgainstBalance)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')

  async function save() {
    setBusy(true)
    setError('')
    try {
      const updated = await leaveTypesApi.update(leaveType.id, {
        color,
        isPaid,
        countsAgainstBalance: counts,
      })
      toast('Leave type updated.')
      onSaved(updated)
      onClose()
    } catch (err) {
      setError(err instanceof ApiError ? err.firstMessage : 'Could not save changes.')
      setBusy(false)
    }
  }

  return (
    <Modal onClose={onClose} width={460}>
      <ModalHeader title="Edit leave type" onClose={onClose} />
      <div style={{ padding: '18px 24px', display: 'flex', flexDirection: 'column', gap: 18 }}>
        <div>
          <div className="field-label">Name</div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 9, fontWeight: 600 }}>
            <span
              style={{
                width: 10,
                height: 10,
                borderRadius: '50%',
                background: COLOR_HEX[color ?? 'Blue'],
                flexShrink: 0,
              }}
            />
            {leaveTypeLabel(leaveType.name)}
          </div>
          <div style={{ color: 'var(--text3)', fontSize: 11.5, marginTop: 6 }}>
            Name is chosen from the catalog and can’t be changed after creation.
          </div>
          {error && <div className="err">{error}</div>}
        </div>

        <div>
          <div style={{ fontSize: 12.5, fontWeight: 600, marginBottom: 8 }}>Color</div>
          <div style={{ display: 'flex', gap: 10, flexWrap: 'wrap' }}>
            {COLOR_ORDER.map((c) => (
              <button
                key={c}
                title={c}
                onClick={() => setColor(c)}
                style={{
                  width: 26,
                  height: 26,
                  borderRadius: '50%',
                  border: 'none',
                  background: COLOR_HEX[c],
                  cursor: 'pointer',
                  boxShadow:
                    color === c ? `0 0 0 2px var(--surface),0 0 0 4px ${COLOR_HEX[c]}` : 'none',
                }}
              />
            ))}
          </div>
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
          <Toggle on={isPaid} onToggle={() => setIsPaid((v) => !v)} label="Paid" />
          <Toggle
            on={counts}
            onToggle={() => setCounts((v) => !v)}
            label="Counts against balance"
          />
        </div>
      </div>
      <div className="modal-foot">
        <button
          className="btn"
          style={{
            background: 'none',
            border: '1px solid var(--border)',
            color: 'var(--text2)',
            padding: '9px 16px',
            fontSize: 13,
          }}
          onClick={onClose}
        >
          Cancel
        </button>
        <button
          className="btn btn-primary"
          style={{ padding: '9px 20px', fontSize: 13 }}
          disabled={busy}
          onClick={save}
        >
          {busy ? 'Saving…' : 'Save changes'}
        </button>
      </div>
    </Modal>
  )
}
