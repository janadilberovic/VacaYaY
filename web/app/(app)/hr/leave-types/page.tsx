'use client'

import { useEffect, useState } from 'react'
import { AddLeaveTypeModal } from '@/components/AddLeaveTypeModal'
import { ConfirmDialog, type ConfirmSpec } from '@/components/ConfirmDialog'
import { Badge } from '@/components/Pill'
import { useToast } from '@/state/toast'
import { leaveTypes as leaveTypesApi } from '@/lib/endpoints'
import { colorHex, leaveTypeLabel } from '@/lib/leave'
import type { LeaveTypeDto, LeaveTypeName } from '@/lib/types'

const COLS = '1.4fr 1.6fr .8fr auto'

function Row({ lt, active, onAct }: { lt: LeaveTypeDto; active: boolean; onAct: () => void }) {
  return (
    <div style={{ display: 'grid', gridTemplateColumns: COLS, gap: 12, alignItems: 'center', padding: '12px 18px', borderBottom: '1px solid var(--border)', fontSize: 13, opacity: active ? 1 : 0.5 }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: 9, fontWeight: 600 }}>
        <span style={{ width: 10, height: 10, borderRadius: '50%', background: colorHex(lt.color), flexShrink: 0 }} />
        {leaveTypeLabel(lt.name)}
      </div>
      <div style={{ display: 'flex', gap: 6, flexWrap: 'wrap' }}>
        <Badge>{lt.isPaid ? 'Paid' : 'Unpaid'}</Badge>
        {lt.countsAgainstBalance && <Badge>Counts against balance</Badge>}
      </div>
      <div>
        <span
          className="pill"
          style={{ fontSize: 11, background: active ? 'var(--pill-approved-bg)' : 'var(--pill-cancelled-bg)', color: active ? 'var(--pill-approved-fg)' : 'var(--pill-cancelled-fg)' }}
        >
          {active ? 'Active' : 'Archived'}
        </span>
      </div>
      <button
        onClick={onAct}
        className="btn"
        style={{ width: 64, background: 'none', border: '1px solid var(--border)', color: 'var(--text2)', borderRadius: 7, padding: '5px 0', fontSize: 11.5 }}
      >
        {active ? 'Archive' : 'Restore'}
      </button>
    </div>
  )
}

export default function LeaveTypesPage() {
  const { toast } = useToast()
  const [active, setActive] = useState<LeaveTypeDto[]>([])
  const [archived, setArchived] = useState<LeaveTypeDto[]>([])
  const [showAdd, setShowAdd] = useState(false)
  const [confirm, setConfirm] = useState<ConfirmSpec | null>(null)

  useEffect(() => {
    leaveTypesApi.all().then((types) => setActive([...types].sort((a, b) => a.name.localeCompare(b.name)))).catch(() => setActive([]))
  }, [])

  function askArchive(lt: LeaveTypeDto) {
    setConfirm({
      title: 'Archive this leave type?',
      message: 'It won’t be selectable on new requests. Existing requests keep it. You can restore it anytime.',
      confirmLabel: 'Archive',
      onConfirm: async () => {
        setConfirm(null)
        try {
          await leaveTypesApi.archive(lt.id)
          setActive((l) => l.filter((x) => x.id !== lt.id))
          setArchived((l) => [...l, lt])
          toast('Leave type archived.')
        } catch {
          toast('Could not archive leave type.', 'error')
        }
      },
    })
  }

  async function restore(lt: LeaveTypeDto) {
    try {
      const restored = await leaveTypesApi.restore(lt.id)
      setArchived((l) => l.filter((x) => x.id !== lt.id))
      setActive((l) => [...l, restored].sort((a, b) => a.name.localeCompare(b.name)))
      toast('Leave type restored.')
    } catch {
      toast('Could not restore leave type.', 'error')
    }
  }

  const usedNames = new Set<LeaveTypeName>([...active, ...archived].map((t) => t.name))

  return (
    <div style={{ animation: 'fade .25s' }}>
      <div className="page-head">
        <div className="page-h">Leave types</div>
        <button className="btn btn-primary" onClick={() => setShowAdd(true)}>＋ Add leave type</button>
      </div>

      <div className="card" style={{ overflow: 'hidden' }}>
        <div style={{ display: 'grid', gridTemplateColumns: COLS, gap: 12, alignItems: 'center', padding: '10px 18px', borderBottom: '1px solid var(--border)' }}>
          {['Name', 'Flags', 'Status'].map((h) => (
            <div key={h} className="section-label" style={{ fontSize: 11 }}>{h}</div>
          ))}
          <div style={{ width: 64 }} />
        </div>
        {active.map((lt) => (
          <Row key={lt.id} lt={lt} active onAct={() => askArchive(lt)} />
        ))}
      </div>

      {archived.length > 0 && (
        <>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, margin: '32px 0 14px' }}>
            <span style={{ fontSize: 13, fontWeight: 650, color: 'var(--text2)' }}>Archived</span>
            <span className="pill" style={{ background: 'var(--surface2)', color: 'var(--text3)' }}>{archived.length}</span>
          </div>
          <div className="card" style={{ overflow: 'hidden' }}>
            {archived.map((lt) => (
              <Row key={lt.id} lt={lt} active={false} onAct={() => restore(lt)} />
            ))}
          </div>
        </>
      )}

      {showAdd && (
        <AddLeaveTypeModal
          usedNames={usedNames}
          onClose={() => setShowAdd(false)}
          onCreated={(t) => setActive((l) => [...l, t].sort((a, b) => a.name.localeCompare(b.name)))}
        />
      )}
      {confirm && <ConfirmDialog spec={confirm} onCancel={() => setConfirm(null)} />}
    </div>
  )
}
