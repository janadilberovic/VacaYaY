'use client'

import { useEffect, useState } from 'react'
import { AddEmployeeModal } from '@/components/AddEmployeeModal'
import { ConfirmDialog, type ConfirmSpec } from '@/components/ConfirmDialog'
import { Avatar } from '@/components/ui'
import { useToast } from '@/state/toast'
import { employees as employeesApi } from '@/lib/endpoints'
import { initials } from '@/lib/format'
import { fmt } from '@/lib/dates'
import type { EmployeeDto } from '@/lib/types'

const PER_PAGE = 8
const COLS = '1.5fr 1.7fr 1fr .8fr .7fr .8fr auto'

function StatusPill({ active }: { active: boolean }) {
  return (
    <span
      className="pill"
      style={{
        fontSize: 11,
        background: active ? 'var(--pill-approved-bg)' : 'var(--pill-cancelled-bg)',
        color: active ? 'var(--pill-approved-fg)' : 'var(--pill-cancelled-fg)',
      }}
    >
      {active ? 'Active' : 'Archived'}
    </span>
  )
}

function Row({ e, onAct }: { e: EmployeeDto; onAct: () => void }) {
  return (
    <div style={{ display: 'grid', gridTemplateColumns: COLS, gap: 12, alignItems: 'center', padding: '12px 18px', borderBottom: '1px solid var(--border)', fontSize: 13, opacity: e.isActive ? 1 : 0.6 }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: 9, minWidth: 0 }}>
        <Avatar text={initials(e.firstName, e.lastName)} />
        <span style={{ fontWeight: 600, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis', textDecoration: e.isActive ? 'none' : 'line-through' }}>
          {e.firstName} {e.lastName}
        </span>
      </div>
      <div style={{ color: 'var(--text2)', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>{e.email}</div>
      <div style={{ color: 'var(--text2)' }}>{e.department || '—'}</div>
      <div style={{ color: 'var(--text2)' }}>{e.hireDate ? fmt(e.hireDate, false) : '—'}</div>
      <div style={{ color: 'var(--text2)' }}>{e.daysOff} days</div>
      <div><StatusPill active={e.isActive} /></div>
      <button
        onClick={onAct}
        className="btn"
        style={{ width: 64, background: 'none', border: '1px solid var(--border)', color: 'var(--text2)', borderRadius: 7, padding: '5px 0', fontSize: 11.5 }}
      >
        {e.isActive ? 'Archive' : 'Restore'}
      </button>
    </div>
  )
}

export default function EmployeesPage() {
  const { toast } = useToast()
  const [list, setList] = useState<EmployeeDto[]>([])
  const [page, setPage] = useState(1)
  const [showAdd, setShowAdd] = useState(false)
  const [confirm, setConfirm] = useState<ConfirmSpec | null>(null)

  useEffect(() => {
    employeesApi.all().then(setList).catch(() => setList([]))
  }, [])

  const active = list.filter((e) => e.isActive)
  const archived = list.filter((e) => !e.isActive)
  const pageCount = Math.max(1, Math.ceil(active.length / PER_PAGE))
  const current = Math.min(page, pageCount)
  const pageRows = active.slice((current - 1) * PER_PAGE, current * PER_PAGE)

  function replace(updated: EmployeeDto) {
    setList((l) => l.map((e) => (e.id === updated.id ? updated : e)))
  }

  function askArchive(e: EmployeeDto) {
    setConfirm({
      title: 'Archive this employee?',
      message: 'Their account will be deactivated and hidden from active lists. You can restore it anytime.',
      confirmLabel: 'Archive',
      onConfirm: async () => {
        setConfirm(null)
        try {
          await employeesApi.archive(e.id)
          replace({ ...e, isActive: false })
          toast('Employee archived.')
        } catch {
          toast('Could not archive employee.', 'error')
        }
      },
    })
  }

  async function restore(e: EmployeeDto) {
    try {
      const updated = await employeesApi.restore(e.id)
      replace(updated)
      toast('Employee restored.')
    } catch {
      toast('Could not restore employee.', 'error')
    }
  }

  return (
    <div style={{ animation: 'fade .25s' }}>
      <div className="page-head">
        <div className="page-h">Employees</div>
        <button className="btn btn-primary" onClick={() => setShowAdd(true)}>＋ Add employee</button>
      </div>

      <div className="card" style={{ overflow: 'hidden' }}>
        <div style={{ display: 'grid', gridTemplateColumns: COLS, gap: 12, alignItems: 'center', padding: '10px 18px', borderBottom: '1px solid var(--border)' }}>
          {['Name', 'Email', 'Department', 'Hired', 'Balance', 'Status'].map((h) => (
            <div key={h} className="section-label" style={{ fontSize: 11 }}>{h}</div>
          ))}
          <div style={{ width: 64 }} />
        </div>
        {pageRows.map((e) => (
          <Row key={e.id} e={e} onAct={() => askArchive(e)} />
        ))}
      </div>

      {pageCount > 1 && (
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginTop: 14 }}>
          <div style={{ color: 'var(--text3)', fontSize: 12.5 }}>
            {(current - 1) * PER_PAGE + 1}–{Math.min(current * PER_PAGE, active.length)} of {active.length}
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
            <button className="btn btn-ghost" style={{ width: 30, height: 30, padding: 0 }} disabled={current <= 1} onClick={() => setPage(current - 1)}>‹</button>
            {Array.from({ length: pageCount }).map((_, i) => {
              const p = i + 1
              const on = p === current
              return (
                <button
                  key={p}
                  onClick={() => setPage(p)}
                  style={{ minWidth: 30, height: 30, border: `1px solid ${on ? 'var(--text)' : 'var(--border)'}`, borderRadius: 8, background: on ? 'var(--text)' : 'var(--surface)', color: on ? 'var(--bg)' : 'var(--text2)', cursor: 'pointer', fontSize: 12.5, fontWeight: 650 }}
                >
                  {p}
                </button>
              )
            })}
            <button className="btn btn-ghost" style={{ width: 30, height: 30, padding: 0 }} disabled={current >= pageCount} onClick={() => setPage(current + 1)}>›</button>
          </div>
        </div>
      )}

      {archived.length > 0 && (
        <>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, margin: '32px 0 14px' }}>
            <span style={{ fontSize: 13, fontWeight: 650, color: 'var(--text2)' }}>Archived accounts</span>
            <span className="pill" style={{ background: 'var(--surface2)', color: 'var(--text3)' }}>{archived.length}</span>
          </div>
          <div className="card" style={{ overflow: 'hidden' }}>
            {archived.map((e) => (
              <Row key={e.id} e={e} onAct={() => restore(e)} />
            ))}
          </div>
        </>
      )}

      {showAdd && (
        <AddEmployeeModal onClose={() => setShowAdd(false)} onCreated={(e) => setList((l) => [e, ...l])} />
      )}
      {confirm && <ConfirmDialog spec={confirm} onCancel={() => setConfirm(null)} />}
    </div>
  )
}
