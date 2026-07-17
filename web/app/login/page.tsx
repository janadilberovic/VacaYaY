'use client'

import { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import { useAuth } from '@/state/auth'
import { ApiError } from '@/lib/api'

export default function LoginPage() {
  const { user, ready, login } = useAuth()
  const router = useRouter()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (ready && user) router.replace('/dashboard')
  }, [ready, user, router])

  async function submit() {
    if (!email.trim() || !password) {
      setError('Enter your email and password.')
      return
    }
    setBusy(true)
    setError('')
    try {
      const res = await login(email.trim(), password)
      router.replace(res.mustChangePassword ? '/change-password' : '/dashboard')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Sign-in failed. Please try again.')
      setBusy(false)
    }
  }

  return (
    <div className="main">
      <div style={{ maxWidth: 380, margin: '8vh auto 0', padding: '0 20px', animation: 'fade .25s' }}>
        <div style={{ fontSize: 17, fontWeight: 700, letterSpacing: '-0.02em', marginBottom: 28, textAlign: 'center' }}>
          Vaca<span style={{ color: 'var(--accent)' }}>YAY</span>
        </div>
        <div className="card" style={{ padding: 26 }}>
          <div style={{ fontSize: 19, fontWeight: 700, letterSpacing: '-0.02em' }}>Welcome back</div>
          <div style={{ color: 'var(--text2)', fontSize: 13, margin: '4px 0 20px' }}>
            Sign in with the credentials HR provisioned for you.
          </div>
          <label htmlFor="l-email" className="field-label">Email</label>
          <input
            id="l-email"
            type="email"
            className="input"
            style={{ marginBottom: 14 }}
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && submit()}
            placeholder="name@ingsoftware.com"
          />
          <label htmlFor="l-pw" className="field-label">Password</label>
          <input
            id="l-pw"
            type="password"
            className="input"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && submit()}
            placeholder="••••••••"
          />
          {error && <div style={{ color: 'var(--pill-rejected-fg)', fontSize: 12, marginTop: 8 }}>{error}</div>}
          <button
            className="btn btn-primary"
            style={{ width: '100%', marginTop: 20, padding: '11px 0' }}
            disabled={busy}
            onClick={submit}
          >
            {busy ? 'Signing in…' : 'Sign in'}
          </button>
        </div>
      </div>
    </div>
  )
}
