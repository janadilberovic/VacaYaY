'use client'

import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { getToken, setToken } from '@/lib/api'
import { auth as authApi, employees } from '@/lib/endpoints'
import type { AuthResponse, AuthUser, EmployeeDto } from '@/lib/types'

const USER_KEY = 'vacayay.user'

interface PendingChange {
  email: string
  currentPassword: string
}

interface AuthCtx {
  user: AuthUser | null
  ready: boolean
  pendingChange: PendingChange | null
  login: (email: string, password: string) => Promise<AuthResponse>
  changePassword: (newPassword: string, confirmNewPassword: string) => Promise<AuthResponse>
  logout: () => Promise<void>
  refreshUser: () => Promise<void>
}

const Ctx = createContext<AuthCtx | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null)
  const [ready, setReady] = useState(false)
  const [pendingChange, setPendingChange] = useState<PendingChange | null>(null)

  useEffect(() => {
    const raw = window.localStorage.getItem(USER_KEY)
    if (raw && getToken()) {
      try {
        setUser(JSON.parse(raw))
      } catch {
        window.localStorage.removeItem(USER_KEY)
      }
    }
    setReady(true)
  }, [])

  function applySession(res: AuthResponse) {
    if (res.accessToken && res.user) {
      setToken(res.accessToken)
      window.localStorage.setItem(USER_KEY, JSON.stringify(res.user))
      setUser(res.user)
      setPendingChange(null)
    }
  }

  async function login(email: string, password: string) {
    const res = await authApi.login(email, password)
    if (res.mustChangePassword) {
      setPendingChange({ email, currentPassword: password })
    } else {
      applySession(res)
    }
    return res
  }

  async function changePassword(newPassword: string, confirmNewPassword: string) {
    if (!pendingChange) throw new Error('No pending password change')
    const res = await authApi.changePassword({
      email: pendingChange.email,
      currentPassword: pendingChange.currentPassword,
      newPassword,
      confirmNewPassword,
    })
    applySession(res)
    return res
  }

  /** Pull the live employee record so balance-affecting changes (approvals, cancellations)
   *  are reflected after login, which is when the cached user was captured. */
  async function refreshUser() {
    if (!getToken()) return
    let me: EmployeeDto
    try {
      me = await employees.me()
    } catch {
      return
    }
    const next: AuthUser = {
      id: me.id,
      firstName: me.firstName,
      lastName: me.lastName,
      email: me.email,
      role: me.role,
      department: me.department,
      jobTitle: me.jobTitle,
      daysOff: me.daysOff,
      profileImageUrl: me.profileImageUrl,
    }
    window.localStorage.setItem(USER_KEY, JSON.stringify(next))
    setUser(next)
  }

  async function logout() {
    try {
      await authApi.logout()
    } catch {
      // Revoking the token server-side is best-effort; clear the client session regardless.
    }
    setToken(null)
    window.localStorage.removeItem(USER_KEY)
    setUser(null)
    setPendingChange(null)
  }

  return (
    <Ctx.Provider value={{ user, ready, pendingChange, login, changePassword, logout, refreshUser }}>
      {children}
    </Ctx.Provider>
  )
}

export function useAuth(): AuthCtx {
  const ctx = useContext(Ctx)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
