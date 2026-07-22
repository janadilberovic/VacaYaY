'use client'

import { useEffect, useState } from 'react'
import { Modal, ModalHeader } from './Modal'
import { useToast } from '@/state/toast'
import { employees } from '@/lib/endpoints'
import { ApiError } from '@/lib/api'
import { fmt } from '@/lib/dates'
import type { EmployeeDto, LegacyEmployeeRosterItem } from '@/lib/types'

const COLS = '28px 1.4fr 1.6fr 1fr .8fr'

function Row({
  e,
  checked,
  onToggle,
}: {
  e: LegacyEmployeeRosterItem
  checked: boolean
  onToggle: () => void
}) {
  return (
    <label
      style={{
        display: 'grid',
        gridTemplateColumns: COLS,
        gap: 12,
        alignItems: 'center',
        padding: '10px 24px',
        borderBottom: '1px solid var(--border)',
        fontSize: 13,
        opacity: e.alreadyImported ? 0.55 : 1,
        cursor: e.alreadyImported ? 'default' : 'pointer',
      }}
    >
      <input type="checkbox" checked={checked} disabled={e.alreadyImported} onChange={onToggle} />
      <span
        style={{
          fontWeight: 600,
          whiteSpace: 'nowrap',
          overflow: 'hidden',
          textOverflow: 'ellipsis',
        }}
      >
        {e.firstName} {e.lastName}
      </span>
      <span
        style={{
          color: 'var(--text2)',
          whiteSpace: 'nowrap',
          overflow: 'hidden',
          textOverflow: 'ellipsis',
        }}
      >
        {e.email}
      </span>
      <span style={{ color: 'var(--text2)' }}>{e.department || '—'}</span>
      {e.alreadyImported ? (
        <span
          className="pill"
          style={{ fontSize: 10.5, background: 'var(--surface2)', color: 'var(--text3)' }}
        >
          Already imported
        </span>
      ) : (
        <span style={{ color: 'var(--text3)' }}>{e.hiredOn ? fmt(e.hiredOn, false) : '—'}</span>
      )}
    </label>
  )
}

export function ImportLegacyModal({
  onClose,
  onImported,
}: {
  onClose: () => void
  onImported: (created: EmployeeDto[]) => void
}) {
  const { toast } = useToast()
  const [rows, setRows] = useState<LegacyEmployeeRosterItem[]>([])
  const [selected, setSelected] = useState<Set<number>>(new Set())
  const [loading, setLoading] = useState(true)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let active = true
    employees
      .legacyRoster()
      .then((r) => {
        if (active) setRows(r)
      })
      .catch((err) => {
        if (active)
          setError(err instanceof ApiError ? err.firstMessage : 'Could not reach the old system.')
      })
      .finally(() => {
        if (active) setLoading(false)
      })
    return () => {
      active = false
    }
  }, [])

  const selectable = rows.filter((e) => !e.alreadyImported)
  const allSelected = selectable.length > 0 && selectable.every((e) => selected.has(e.legacyId))

  function toggle(legacyId: number) {
    setSelected((prev) => {
      const next = new Set(prev)
      if (next.has(legacyId)) next.delete(legacyId)
      else next.add(legacyId)
      return next
    })
  }

  function toggleAll() {
    setSelected(allSelected ? new Set() : new Set(selectable.map((e) => e.legacyId)))
  }

  async function runImport() {
    setBusy(true)
    setError(null)
    try {
      const res = await employees.importLegacy([...selected])
      onImported(res.importedEmployees)
      setRows(res.roster)
      setSelected(new Set())
      const skipped = res.skipped + res.invalid + res.notFound
      toast(
        `Imported ${res.imported} employee${res.imported === 1 ? '' : 's'}${skipped ? `. ${skipped} skipped.` : '.'}`,
      )
      onClose()
    } catch (err) {
      setError(err instanceof ApiError ? err.firstMessage : 'Could not import employees.')
      setBusy(false)
    }
  }

  // Closing mid-import would leave runImport setting state on an unmounted modal.
  function close() {
    if (!busy) onClose()
  }

  return (
    <Modal onClose={close} width={640}>
      <ModalHeader title="Load existing users from old system" onClose={close} />

      <div style={{ padding: '10px 24px 0', color: 'var(--text2)', fontSize: 12.5 }}>
        Pick who to bring across. Everyone starts on the standard leave balance and gets a temporary
        password — issue it from the employee’s Reset password action.
      </div>

      {loading ? (
        <div style={{ padding: '28px 24px', color: 'var(--text3)', fontSize: 13 }}>
          Loading the old system…
        </div>
      ) : rows.length === 0 ? (
        <div style={{ padding: '28px 24px', color: 'var(--text3)', fontSize: 13 }}>
          The old system has no employees to show.
        </div>
      ) : (
        <>
          <div
            style={{
              display: 'grid',
              gridTemplateColumns: COLS,
              gap: 12,
              alignItems: 'center',
              padding: '14px 24px 10px',
              borderBottom: '1px solid var(--border)',
            }}
          >
            <input
              type="checkbox"
              aria-label="Select all importable employees"
              checked={allSelected}
              disabled={selectable.length === 0}
              onChange={toggleAll}
            />
            {['Name', 'Email', 'Department', 'Hired'].map((h) => (
              <div key={h} className="section-label" style={{ fontSize: 11 }}>
                {h}
              </div>
            ))}
          </div>

          <div style={{ maxHeight: '46vh', overflowY: 'auto' }}>
            {rows.map((e) => (
              <Row
                key={e.legacyId}
                e={e}
                checked={e.alreadyImported || selected.has(e.legacyId)}
                onToggle={() => toggle(e.legacyId)}
              />
            ))}
          </div>
        </>
      )}

      {error && (
        <div className="err" style={{ padding: '12px 24px 0' }}>
          {error}
        </div>
      )}

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
          disabled={busy}
          onClick={close}
        >
          Cancel
        </button>
        <button
          className="btn btn-primary"
          style={{ padding: '9px 20px', fontSize: 13 }}
          disabled={busy || selected.size === 0}
          onClick={runImport}
        >
          {busy ? 'Importing…' : `Import ${selected.size} selected`}
        </button>
      </div>
    </Modal>
  )
}
