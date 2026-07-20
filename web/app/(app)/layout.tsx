'use client'

import { useEffect, type ReactNode } from 'react'
import { useRouter } from 'next/navigation'
import { useAuth } from '@/state/auth'
import { LeaveModalProvider } from '@/state/leaveModal'
import { Sidebar } from '@/components/Sidebar'
import { RequestLeaveModal } from '@/components/RequestLeaveModal'

export default function AppLayout({ children }: { children: ReactNode }) {
  const { user, ready } = useAuth()
  const router = useRouter()

  useEffect(() => {
    if (ready && !user) router.replace('/login')
  }, [ready, user, router])

  if (!ready || !user) return null

  return (
    <LeaveModalProvider>
      <div className="app">
        <Sidebar />
        <div className="main">
          <div className="main-inner">{children}</div>
        </div>
        <RequestLeaveModal />
      </div>
    </LeaveModalProvider>
  )
}
