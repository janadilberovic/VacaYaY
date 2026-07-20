'use client'

import { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import { useAuth } from '@/state/auth'
import { useToast } from '@/state/toast'
import { ApiError } from '@/lib/api'

export default function ChangePasswordPage() {
  const { ready, pendingChange, changePassword } = useAuth()
  const { toast } = useToast()
  const router = useRouter()
  const [pw, setPw] = useState('')
  const [confirm, setConfirm] = useState('')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (ready && !pendingChange) router.replace('/login')
  }, [ready, pendingChange, router])

  const current = pendingChange?.currentPassword ?? ''
  const rules = [
    { label: 'At least 8 characters', ok: pw.length >= 8 },
    { label: 'An uppercase letter', ok: /[A-Z]/.test(pw) },
    { label: 'A lowercase letter', ok: /[a-z]/.test(pw) },
    { label: 'A digit', ok: /\d/.test(pw) },
    { label: 'Different from your current password', ok: !!pw && pw !== current },
    { label: 'Passwords match', ok: !!confirm && pw === confirm },
  ]
  const canSubmit = rules.every((r) => r.ok)

  async function submit() {
    if (!canSubmit) return
    setBusy(true)
    setError('')
    try {
      await changePassword(pw, confirm)
      toast('Password changed — welcome!')
      router.replace('/dashboard')
    } catch (err) {
      setError(err instanceof ApiError ? err.firstMessage : 'Could not change password. Please try again.')
      setBusy(false)
    }
  }

  return (
    <div className="main">
      <div style={{ maxWidth: 380, margin: '8vh auto 0', padding: '0 20px', animation: 'fade .25s' }}>
        <div className="card" style={{ padding: 26 }}>
          <div style={{ fontSize: 19, fontWeight: 700, letterSpacing: '-0.02em' }}>Set a new password</div>
          <div style={{ color: 'var(--text2)', fontSize: 13, margin: '4px 0 20px' }}>
            This is your first login — choose a new password before continuing.
          </div>
          <label htmlFor="p-a" className="field-label">New password</label>
          <input id="p-a" type="password" className="input" style={{ marginBottom: 14 }} value={pw} onChange={(e) => setPw(e.target.value)} />
          <label htmlFor="p-b" className="field-label">Confirm password</label>
          <input id="p-b" type="password" className="input" value={confirm} onChange={(e) => setConfirm(e.target.value)} />
          <div
            style={{
              display: 'flex',
              flexDirection: 'column',
              gap: 6,
              margin: '16px 0 4px',
              background: 'var(--surface2)',
              borderRadius: 10,
              padding: '12px 14px',
            }}
          >
            {rules.map((r) => (
              <div
                key={r.label}
                style={{ display: 'flex', alignItems: 'center', gap: 8, fontSize: 12.5, color: r.ok ? 'var(--pill-approved-fg)' : 'var(--text3)' }}
              >
                <span style={{ width: 14, textAlign: 'center' }}>{r.ok ? '✓' : '○'}</span>
                {r.label}
              </div>
            ))}
          </div>
          {error && <div style={{ color: 'var(--pill-rejected-fg)', fontSize: 12, marginTop: 8 }}>{error}</div>}
          <button
            className="btn"
            style={{
              width: '100%',
              marginTop: 16,
              padding: '11px 0',
              background: canSubmit ? 'var(--accent)' : 'var(--surface2)',
              color: canSubmit ? '#fff' : 'var(--text3)',
              border: 'none',
              cursor: canSubmit ? 'pointer' : 'default',
            }}
            disabled={busy || !canSubmit}
            onClick={submit}
          >
            {busy ? 'Changing…' : 'Change password'}
          </button>
        </div>
      </div>
    </div>
  )
}
