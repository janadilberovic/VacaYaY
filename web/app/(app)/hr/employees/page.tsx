'use client'

import { useCallback, useEffect, useState } from 'react'
import { AddEmployeeModal } from '@/components/AddEmployeeModal'
import { ImportLegacyModal } from '@/components/ImportLegacyModal'
import { ConfirmDialog, type ConfirmSpec } from '@/components/ConfirmDialog'
import { Pagination } from '@/components/Pagination'
import { Avatar } from '@/components/ui'
import { useToast } from '@/state/toast'
import { employees as employeesApi } from '@/lib/endpoints'
import { initials } from '@/lib/format'
import { fmt } from '@/lib/dates'
import type { EmployeeDto, PagedResult } from '@/lib/types'

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
    <div
      style={{
        display: 'grid',
        gridTemplateColumns: COLS,
        gap: 12,
        alignItems: 'center',
        padding: '12px 18px',
        borderBottom: '1px solid var(--border)',
        fontSize: 13,
        opacity: e.isActive ? 1 : 0.6,
      }}
    >
      <div style={{ display: 'flex', alignItems: 'center', gap: 9, minWidth: 0 }}>
        <Avatar text={initials(e.firstName, e.lastName)} />
        <span
          style={{
            fontWeight: 600,
            whiteSpace: 'nowrap',
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            textDecoration: e.isActive ? 'none' : 'line-through',
          }}
        >
          {e.firstName} {e.lastName}
        </span>
      </div>
      <div
        style={{
          color: 'var(--text2)',
          whiteSpace: 'nowrap',
          overflow: 'hidden',
          textOverflow: 'ellipsis',
        }}
      >
        {e.email}
      </div>
      <div style={{ color: 'var(--text2)' }}>{e.department || '—'}</div>
      <div style={{ color: 'var(--text2)' }}>{e.hireDate ? fmt(e.hireDate, false) : '—'}</div>
      <div style={{ color: 'var(--text2)' }}>{e.daysOff} days</div>
      <div>
        <StatusPill active={e.isActive} />
      </div>
      <button
        onClick={onAct}
        className="btn"
        style={{
          width: 64,
          background: 'none',
          border: '1px solid var(--border)',
          color: 'var(--text2)',
          borderRadius: 7,
          padding: '5px 0',
          fontSize: 11.5,
        }}
      >
        {e.isActive ? 'Archive' : 'Restore'}
      </button>
    </div>
  )
}

export default function EmployeesPage() {
  const { toast } = useToast()
  const [data, setData] = useState<PagedResult<EmployeeDto> | null>(null)
  const [page, setPage] = useState(1)
  const [archived, setArchived] = useState(false)
  const [showAdd, setShowAdd] = useState(false)
  const [showImport, setShowImport] = useState(false)
  const [confirm, setConfirm] = useState<ConfirmSpec | null>(null)

  const load = useCallback(async () => {
    try {
      const res = await employeesApi.list({ page, pageSize: PER_PAGE, archived })
      // Archiving the last row on the last page leaves us past the end — step back.
      if (res.items.length === 0 && res.page > 1) {
        setPage(Math.max(1, res.totalPages))
        return
      }
      setData(res)
    } catch {
      setData(null)
    }
  }, [page, archived])

  useEffect(() => {
    load()
  }, [load])

  function reload() {
    if (page === 1) load()
    else setPage(1)
  }

  const rows = data?.items ?? []
  const totalCount = data?.totalCount ?? 0
  const pageCount = Math.max(1, data?.totalPages ?? 1)
  const current = data?.page ?? page

  function askArchive(e: EmployeeDto) {
    setConfirm({
      title: 'Archive this employee?',
      message:
        'Their account will be deactivated and hidden from active lists. You can restore it anytime.',
      confirmLabel: 'Archive',
      onConfirm: async () => {
        setConfirm(null)
        try {
          await employeesApi.archive(e.id)
          await load()
          toast('Employee archived.')
        } catch {
          toast('Could not archive employee.', 'error')
        }
      },
    })
  }

  async function restore(e: EmployeeDto) {
    try {
      await employeesApi.restore(e.id)
      await load()
      toast('Employee restored.')
    } catch {
      toast('Could not restore employee.', 'error')
    }
  }

  return (
    <div style={{ animation: 'fade .25s' }}>
      <div className="page-head">
        <div className="page-h">Employees</div>
        <div style={{ display: 'flex', gap: 10 }}>
          <button
            className="btn"
            style={{
              background: 'none',
              border: '1px solid var(--border)',
              color: archived ? 'var(--text)' : 'var(--text2)',
            }}
            onClick={() => {
              setArchived((a) => !a)
              setPage(1)
            }}
          >
            {archived ? 'Show active' : 'Show archived'}
          </button>
          <button
            className="btn"
            style={{ background: 'none', border: '1px solid var(--border)', color: 'var(--text2)' }}
            onClick={() => setShowImport(true)}
          >
            Load existing users from old system
          </button>
          <button className="btn btn-primary" onClick={() => setShowAdd(true)}>
            ＋ Add employee
          </button>
        </div>
      </div>

      <div className="card" style={{ overflow: 'hidden' }}>
        <div
          style={{
            display: 'grid',
            gridTemplateColumns: COLS,
            gap: 12,
            alignItems: 'center',
            padding: '10px 18px',
            borderBottom: '1px solid var(--border)',
          }}
        >
          {['Name', 'Email', 'Department', 'Hired', 'Balance', 'Status'].map((h) => (
            <div key={h} className="section-label" style={{ fontSize: 11 }}>
              {h}
            </div>
          ))}
          <div style={{ width: 64 }} />
        </div>
        {rows.map((e) => (
          <Row key={e.id} e={e} onAct={() => (archived ? restore(e) : askArchive(e))} />
        ))}
        {rows.length === 0 && (
          <div style={{ padding: '18px', color: 'var(--text3)', fontSize: 13 }}>
            {archived ? 'No archived accounts.' : 'No employees yet.'}
          </div>
        )}
      </div>

      <Pagination
        page={current}
        pageSize={PER_PAGE}
        pageCount={pageCount}
        totalCount={totalCount}
        onPage={setPage}
      />

      {showImport && <ImportLegacyModal onClose={() => setShowImport(false)} onImported={reload} />}
      {showAdd && <AddEmployeeModal onClose={() => setShowAdd(false)} onCreated={reload} />}
      {confirm && <ConfirmDialog spec={confirm} onCancel={() => setConfirm(null)} />}
    </div>
  )
}
