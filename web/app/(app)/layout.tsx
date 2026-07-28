'use client'

import { useEffect, useState, type ReactNode } from 'react'
import { usePathname, useRouter } from 'next/navigation'
import { useAuth } from '@/state/auth'
import { LeaveModalProvider } from '@/state/leaveModal'
import { Sidebar } from '@/components/Sidebar'
import { RequestLeaveModal } from '@/components/RequestLeaveModal'

export default function AppLayout({ children }: { children: ReactNode }) {
  const { user, ready, refreshUser } = useAuth()
  const router = useRouter()
  const pathname = usePathname()
  const [navOpen, setNavOpen] = useState(false)

  useEffect(() => {
    if (ready && !user) router.replace('/login')
  }, [ready, user, router])

  useEffect(() => {
    setNavOpen(false)
  }, [pathname])

  // The cached user is captured at login; approvals and cancellations change the
  // balance afterwards. Re-sync it from the server on every navigation and tab
  // refocus so the number is never stale.
  useEffect(() => {
    if (!ready || !user) return
    refreshUser()
    const onFocus = () => refreshUser()
    window.addEventListener('focus', onFocus)
    return () => window.removeEventListener('focus', onFocus)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [ready, pathname])

  if (!ready || !user) return null

  return (
    <LeaveModalProvider>
      <div className="app">
        <div className="topbar">
          <button className="burger" aria-label="Open menu" onClick={() => setNavOpen(true)}>
            ☰
          </button>
          <div style={{ fontSize: 16, fontWeight: 700, letterSpacing: '-0.02em' }}>
            Vaca<span style={{ color: 'var(--accent)' }}>YAY</span>
          </div>
        </div>
        {navOpen && <div className="scrim" onClick={() => setNavOpen(false)} />}
        <Sidebar open={navOpen} onNavigate={() => setNavOpen(false)} />
        <div className="main">
          <div className="main-inner">{children}</div>
        </div>
        <RequestLeaveModal />
      </div>
    </LeaveModalProvider>
  )
}
