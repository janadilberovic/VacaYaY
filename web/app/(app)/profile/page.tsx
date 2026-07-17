'use client'

import { useEffect, useState } from 'react'
import { employees } from '@/lib/endpoints'
import { initials } from '@/lib/format'
import { fmt } from '@/lib/dates'
import type { EmployeeDto } from '@/lib/types'

function Field({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <div className="section-label" style={{ marginBottom: 3 }}>{label}</div>
      {value}
    </div>
  )
}

export default function ProfilePage() {
  const [me, setMe] = useState<EmployeeDto | null>(null)

  useEffect(() => {
    employees.me().then(setMe).catch(() => setMe(null))
  }, [])

  if (!me) return <div style={{ color: 'var(--text3)' }}>Loading…</div>

  return (
    <div style={{ animation: 'fade .25s' }}>
      <div className="page-h" style={{ marginBottom: 24 }}>Profile</div>
      <div className="card" style={{ padding: 26, maxWidth: 560 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 16, marginBottom: 22 }}>
          <div
            style={{
              width: 56,
              height: 56,
              borderRadius: '50%',
              background: 'var(--accent)',
              color: '#fff',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              fontSize: 20,
              fontWeight: 600,
            }}
          >
            {initials(me.firstName, me.lastName)}
          </div>
          <div>
            <div style={{ fontSize: 17, fontWeight: 700 }}>{me.firstName} {me.lastName}</div>
            <div style={{ color: 'var(--text3)', fontSize: 13 }}>{me.email}</div>
          </div>
        </div>
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px 24px', fontSize: 13.5 }}>
          <Field label="Department" value={me.department || '—'} />
          <Field label="Job title" value={me.jobTitle || '—'} />
          <Field label="Hire date" value={me.hireDate ? fmt(me.hireDate) : '—'} />
          <Field label="Role" value={me.role === 'HR' ? 'HR Manager' : 'Employee'} />
          <Field label="Days-off balance" value={String(me.daysOff)} />
          <Field label="Status" value={me.isActive ? 'Active' : 'Inactive'} />
        </div>
      </div>
    </div>
  )
}
