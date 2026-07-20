'use client'

import { useState } from 'react'
import { Modal, ModalHeader } from './Modal'
import { Toggle } from './Toggle'
import { useToast } from '@/state/toast'
import { leaveTypes as leaveTypesApi } from '@/lib/endpoints'
import { ApiError } from '@/lib/api'
import { COLOR_HEX, COLOR_ORDER, LEAVE_TYPE_NAMES, leaveTypeLabel } from '@/lib/leave'
import type { LeaveColor, LeaveTypeDto, LeaveTypeName } from '@/lib/types'

export function AddLeaveTypeModal({
  usedNames,
  onClose,
  onCreated,
}: {
  usedNames: Set<LeaveTypeName>
  onClose: () => void
  onCreated: (t: LeaveTypeDto) => void
}) {
  const { toast } = useToast()
  const firstFree = LEAVE_TYPE_NAMES.find((n) => !usedNames.has(n)) ?? LEAVE_TYPE_NAMES[0]
  const [name, setName] = useState<LeaveTypeName>(firstFree)
  const [color, setColor] = useState<LeaveColor>('Blue')
  const [isPaid, setIsPaid] = useState(true)
  const [counts, setCounts] = useState(true)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')

  async function create() {
    setBusy(true)
    setError('')
    try {
      const created = await leaveTypesApi.create({ name, color, isPaid, countsAgainstBalance: counts })
      toast('Leave type created.')
      onCreated(created)
      onClose()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Could not create leave type.')
      setBusy(false)
    }
  }

  return (
    <Modal onClose={onClose} width={460}>
      <ModalHeader title="Add leave type" onClose={onClose} />
      <div style={{ padding: '18px 24px', display: 'flex', flexDirection: 'column', gap: 18 }}>
        <div>
          <label htmlFor="lt-name" className="field-label">Name</label>
          <select id="lt-name" className="select" value={name} onChange={(e) => setName(e.target.value as LeaveTypeName)}>
            {LEAVE_TYPE_NAMES.map((n) => (
              <option key={n} value={n} disabled={usedNames.has(n)}>
                {leaveTypeLabel(n)}
              </option>
            ))}
          </select>
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
                  boxShadow: color === c ? `0 0 0 2px var(--surface),0 0 0 4px ${COLOR_HEX[c]}` : 'none',
                }}
              />
            ))}
          </div>
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
          <Toggle on={isPaid} onToggle={() => setIsPaid((v) => !v)} label="Paid" />
          <Toggle on={counts} onToggle={() => setCounts((v) => !v)} label="Counts against balance" />
        </div>
      </div>
      <div className="modal-foot">
        <button
          className="btn"
          style={{ background: 'none', border: '1px solid var(--border)', color: 'var(--text2)', padding: '9px 16px', fontSize: 13 }}
          onClick={onClose}
        >
          Cancel
        </button>
        <button className="btn btn-primary" style={{ padding: '9px 20px', fontSize: 13 }} disabled={busy} onClick={create}>
          {busy ? 'Creating…' : 'Create leave type'}
        </button>
      </div>
    </Modal>
  )
}
