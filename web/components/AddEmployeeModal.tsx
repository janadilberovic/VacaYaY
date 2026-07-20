'use client'

import { useState } from 'react'
import { Modal, ModalHeader } from './Modal'
import { useToast } from '@/state/toast'
import { employees } from '@/lib/endpoints'
import { ApiError } from '@/lib/api'
import { todayISO } from '@/lib/dates'
import type { EmployeeDto, UserRole } from '@/lib/types'

interface Errors {
  firstName?: string
  lastName?: string
  email?: string
  form?: string
}

export function AddEmployeeModal({ onClose, onCreated }: { onClose: () => void; onCreated: (e: EmployeeDto) => void }) {
  const { toast } = useToast()
  const [f, setF] = useState({
    firstName: '',
    lastName: '',
    email: '',
    department: '',
    jobTitle: '',
    role: 'Employee' as UserRole,
    hireDate: todayISO(),
    daysOff: 20,
  })
  const [errs, setErrs] = useState<Errors>({})
  const [busy, setBusy] = useState(false)
  const [tempPassword, setTempPassword] = useState<string | null>(null)
  const [copied, setCopied] = useState(false)

  const set = <K extends keyof typeof f>(key: K, value: (typeof f)[K]) => {
    setF((prev) => ({ ...prev, [key]: value }))
    setErrs((e) => ({ ...e, [key]: undefined }))
  }

  function validate(): Errors {
    const e: Errors = {}
    if (!f.firstName.trim()) e.firstName = 'Required.'
    if (!f.lastName.trim()) e.lastName = 'Required.'
    if (!f.email.trim() || !/^\S+@\S+\.\S+$/.test(f.email)) e.email = 'Enter a valid email.'
    return e
  }

  async function create() {
    const found = validate()
    if (Object.keys(found).length) {
      setErrs(found)
      return
    }
    setBusy(true)
    setErrs({})
    try {
      const res = await employees.create({
        firstName: f.firstName.trim(),
        lastName: f.lastName.trim(),
        email: f.email.trim(),
        role: f.role,
        department: f.department.trim() || null,
        jobTitle: f.jobTitle.trim() || null,
        hireDate: f.hireDate || null,
        employmentStartDate: null,
        employmentEndDate: null,
        daysOff: Number(f.daysOff) || 0,
      })
      onCreated(res.employee)
      setTempPassword(res.tempPassword)
    } catch (err) {
      setErrs({ form: err instanceof ApiError ? (err.detail || err.message) : 'Could not create employee.' })
      setBusy(false)
    }
  }

  function copy() {
    if (tempPassword) navigator.clipboard?.writeText(tempPassword).catch(() => {})
    setCopied(true)
    setTimeout(() => setCopied(false), 2000)
  }

  function finish() {
    toast('Employee created.')
    onClose()
  }

  const input = (id: string, label: string, key: 'firstName' | 'lastName' | 'email' | 'department' | 'jobTitle', type = 'text') => (
    <div>
      <label htmlFor={id} className="field-label">{label}</label>
      <input
        id={id}
        type={type}
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
      {tempPassword ? (
        <div style={{ padding: '26px 26px 22px' }}>
          <div style={{ fontSize: 16, fontWeight: 700, marginBottom: 6 }}>One-time temporary password</div>
          <div style={{ color: 'var(--text2)', fontSize: 13, marginBottom: 18 }}>
            Share it securely — it’s shown only once. They’ll set their own password on first login.
          </div>
          <div
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: 12,
              border: '1px dashed var(--border)',
              borderRadius: 12,
              padding: '16px 18px',
              background: 'var(--surface2)',
            }}
          >
            <div style={{ flex: 1, fontFamily: 'ui-monospace,monospace', fontSize: 17, fontWeight: 600, letterSpacing: '.04em' }}>{tempPassword}</div>
            <button className="btn btn-ghost" style={{ padding: '7px 14px', fontSize: 12 }} onClick={copy}>
              {copied ? 'Copied ✓' : 'Copy'}
            </button>
          </div>
          <div style={{ display: 'flex', justifyContent: 'flex-end', marginTop: 20 }}>
            <button className="btn btn-primary" style={{ padding: '9px 20px', fontSize: 13 }} onClick={finish}>Done</button>
          </div>
        </div>
      ) : (
        <>
          <ModalHeader title="Add employee" onClose={onClose} />
          <div style={{ padding: '18px 24px', display: 'flex', flexDirection: 'column', gap: 14 }}>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
              {input('e-first', 'First name', 'firstName')}
              {input('e-last', 'Last name', 'lastName')}
            </div>
            {input('e-email', 'Email', 'email', 'email')}
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
              {input('e-dept', 'Department', 'department')}
              {input('e-title', 'Job title', 'jobTitle')}
            </div>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: 12 }}>
              <div>
                <label htmlFor="e-role" className="field-label">Role</label>
                <select id="e-role" className="select" value={f.role} onChange={(e) => set('role', e.target.value as UserRole)}>
                  <option value="Employee">Employee</option>
                  <option value="HR">HR</option>
                </select>
              </div>
              <div>
                <label htmlFor="e-hired" className="field-label">Hire date</label>
                <input id="e-hired" type="date" className="input" value={f.hireDate} onChange={(e) => set('hireDate', e.target.value)} />
              </div>
              <div>
                <label htmlFor="e-bal" className="field-label">Balance</label>
                <input id="e-bal" type="number" min={0} max={40} className="input" value={f.daysOff} onChange={(e) => set('daysOff', Number(e.target.value))} />
              </div>
            </div>
            {errs.form && <div className="err">{errs.form}</div>}
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
              {busy ? 'Creating…' : 'Create employee'}
            </button>
          </div>
        </>
      )}
    </Modal>
  )
}
