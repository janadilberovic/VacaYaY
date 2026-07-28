'use client'

import { useCallback, useEffect, useState } from 'react'
import { AddEmployeeModal } from '@/components/AddEmployeeModal'
import { EditEmployeeModal } from '@/components/EditEmployeeModal'
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

const ACTIONS_WIDTH = 126

const actionBtn = {
  background: 'none',
  border: '1px solid var(--border)',
  color: 'var(--text2)',
  borderRadius: 7,
  padding: '5px 0',
  fontSize: 11.5,
}

function Row({
  e,
  onEdit,
  onAct,
}: {
  e: EmployeeDto
  /** Absent for archived rows — the service reads through the soft-delete filter, so editing one 404s. */
  onEdit?: () => void
  onAct: () => void
}) {
  return (
    <div
      className="tbl-row row-emp"
      style={{
        padding: '12px 18px',
        borderBottom: '1px solid var(--border)',
        fontSize: 13,
        opacity: e.isActive ? 1 : 0.6,
      }}
    >
      <div
        className="c-name"
        style={{ display: 'flex', alignItems: 'center', gap: 9, minWidth: 0 }}
      >
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
        className="c-email"
        style={{
          color: 'var(--text2)',
          whiteSpace: 'nowrap',
          overflow: 'hidden',
          textOverflow: 'ellipsis',
        }}
      >
        {e.email}
      </div>
      <div className="c-meta sub-cells" style={{ color: 'var(--text2)' }}>
        <div>{e.department || '—'}</div>
        <div>{e.hireDate ? fmt(e.hireDate, false) : '—'}</div>
        <div>{e.daysOff} days</div>
      </div>
      <div className="c-status">
        <StatusPill active={e.isActive} />
      </div>
      <div
        className="c-action"
        style={{ display: 'flex', gap: 6, width: ACTIONS_WIDTH, justifyContent: 'flex-end' }}
      >
        {onEdit && (
          <button onClick={onEdit} className="btn" style={{ ...actionBtn, width: 54 }}>
            Edit
          </button>
        )}
        <button onClick={onAct} className="btn" style={{ ...actionBtn, width: 66 }}>
          {e.isActive ? 'Archive' : 'Restore'}
        </button>
      </div>
    </div>
  )
}

export default function EmployeesPage() {
  const { toast } = useToast()
  const [data, setData] = useState<PagedResult<EmployeeDto> | null>(null)
  const [page, setPage] = useState(1)
  const [archived, setArchived] = useState(false)
  const [showAdd, setShowAdd] = useState(false)
  const [editing, setEditing] = useState<EmployeeDto | null>(null)
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
        <div style={{ display: 'flex', gap: 10, flexWrap: 'wrap' }}>
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
          className="tbl-head row-emp"
          style={{ padding: '10px 18px', borderBottom: '1px solid var(--border)' }}
        >
          {['Name', 'Email', 'Department', 'Hired', 'Balance', 'Status'].map((h) => (
            <div key={h} className="section-label" style={{ fontSize: 11 }}>
              {h}
            </div>
          ))}
          <div style={{ width: ACTIONS_WIDTH }} />
        </div>
        {rows.map((e) => (
          <Row
            key={e.id}
            e={e}
            onEdit={archived ? undefined : () => setEditing(e)}
            onAct={() => (archived ? restore(e) : askArchive(e))}
          />
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
      {editing && (
        // The list is ordered by name, so a rename can move the row — refetch instead of patching.
        <EditEmployeeModal
          employee={editing}
          onClose={() => setEditing(null)}
          onSaved={() => load()}
        />
      )}
      {confirm && <ConfirmDialog spec={confirm} onCancel={() => setConfirm(null)} />}
    </div>
  )
}
