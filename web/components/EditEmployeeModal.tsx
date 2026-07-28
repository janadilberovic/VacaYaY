'use client'

import { useState } from 'react'
import { Modal, ModalHeader } from './Modal'
import { DatePicker } from './DatePicker'
import { useToast } from '@/state/toast'
import { employees } from '@/lib/endpoints'
import { ApiError } from '@/lib/api'
import { fmt, isoDate } from '@/lib/dates'
import type { EmployeeDto } from '@/lib/types'

interface Errors {
  firstName?: string
  lastName?: string
  form?: string
}

function ReadOnly({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <div className="section-label" style={{ marginBottom: 3 }}>
        {label}
      </div>
      <div style={{ color: 'var(--text2)', fontSize: 13 }}>{value}</div>
    </div>
  )
}

export function EditEmployeeModal({
  employee,
  onClose,
  onSaved,
}: {
  employee: EmployeeDto
  onClose: () => void
  onSaved: (e: EmployeeDto) => void
}) {
  const { toast } = useToast()
  const [f, setF] = useState({
    firstName: employee.firstName,
    lastName: employee.lastName,
    department: employee.department ?? '',
    jobTitle: employee.jobTitle ?? '',
    employmentStartDate: employee.employmentStartDate ? isoDate(employee.employmentStartDate) : '',
    employmentEndDate: employee.employmentEndDate ? isoDate(employee.employmentEndDate) : '',
    daysOff: employee.daysOff,
  })
  const [errs, setErrs] = useState<Errors>({})
  const [busy, setBusy] = useState(false)

  const set = <K extends keyof typeof f>(key: K, value: (typeof f)[K]) => {
    setF((prev) => ({ ...prev, [key]: value }))
    setErrs((e) => ({ ...e, [key]: undefined }))
  }

  function validate(): Errors {
    const e: Errors = {}
    if (!f.firstName.trim()) e.firstName = 'Required.'
    if (!f.lastName.trim()) e.lastName = 'Required.'
    return e
  }

  async function save() {
    const found = validate()
    if (Object.keys(found).length) {
      setErrs(found)
      return
    }
    setBusy(true)
    setErrs({})
    try {
      const updated = await employees.update(employee.id, {
        firstName: f.firstName.trim(),
        lastName: f.lastName.trim(),
        department: f.department.trim() || null,
        jobTitle: f.jobTitle.trim() || null,
        employmentStartDate: f.employmentStartDate || null,
        employmentEndDate: f.employmentEndDate || null,
        daysOff: Number(f.daysOff) || 0,
        // The update replaces every field, so carry this one through — archiving is a
        // separate action and must not flip as a side effect of an edit.
        isActive: employee.isActive,
      })
      toast('Employee updated.')
      onSaved(updated)
      onClose()
    } catch (err) {
      setErrs({ form: err instanceof ApiError ? err.firstMessage : 'Could not save changes.' })
      setBusy(false)
    }
  }

  const input = (
    id: string,
    label: string,
    key: 'firstName' | 'lastName' | 'department' | 'jobTitle',
  ) => (
    <div>
      <label htmlFor={id} className="field-label">
        {label}
      </label>
      <input
        id={id}
        className="input"
        style={errs[key as keyof Errors] ? { borderColor: 'var(--pill-rejected-fg)' } : undefined}
        value={f[key]}
        onChange={(e) => set(key, e.target.value)}
      />
      {errs[key as keyof Errors] && <div className="err">{errs[key as keyof Errors]}</div>}
    </div>
  )

  return (
    <Modal onClose={onClose} width={480}>
      <ModalHeader title="Edit employee" onClose={onClose} />
      <div style={{ padding: '18px 24px', display: 'flex', flexDirection: 'column', gap: 14 }}>
        <div
          className="grid-3"
          style={{ background: 'var(--surface2)', borderRadius: 10, padding: '12px 14px' }}
        >
          <ReadOnly label="Email" value={employee.email} />
          <ReadOnly label="Role" value={employee.role === 'HR' ? 'HR Manager' : 'Employee'} />
          <ReadOnly label="Hired" value={employee.hireDate ? fmt(employee.hireDate) : '—'} />
        </div>

        <div className="grid-2">
          {input('ee-first', 'First name', 'firstName')}
          {input('ee-last', 'Last name', 'lastName')}
        </div>
        <div className="grid-2">
          {input('ee-dept', 'Department', 'department')}
          {input('ee-title', 'Job title', 'jobTitle')}
        </div>
        <div className="grid-3">
          <div>
            <label htmlFor="ee-start" className="field-label">
              Employment start
            </label>
            <DatePicker
              id="ee-start"
              value={f.employmentStartDate}
              onChange={(iso) => set('employmentStartDate', iso)}
            />
          </div>
          <div>
            <label htmlFor="ee-end" className="field-label">
              Employment end
            </label>
            <DatePicker
              id="ee-end"
              value={f.employmentEndDate}
              onChange={(iso) => set('employmentEndDate', iso)}
            />
          </div>
          <div>
            <label htmlFor="ee-bal" className="field-label">
              Balance
            </label>
            <input
              id="ee-bal"
              type="number"
              min={0}
              max={40}
              className="input"
              value={f.daysOff}
              onChange={(e) => set('daysOff', Number(e.target.value))}
            />
          </div>
        </div>
        {errs.form && <div className="err">{errs.form}</div>}
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
