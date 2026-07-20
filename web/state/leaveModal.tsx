'use client'

import { createContext, useContext, useState, type ReactNode } from 'react'

interface LeaveModalCtx {
  open: () => void
  close: () => void
  isOpen: boolean
  /** Bumped after a successful submission so lists can refetch. */
  refreshKey: number
  notifyCreated: () => void
}

const Ctx = createContext<LeaveModalCtx | null>(null)

export function LeaveModalProvider({ children }: { children: ReactNode }) {
  const [isOpen, setOpen] = useState(false)
  const [refreshKey, setRefreshKey] = useState(0)

  return (
    <Ctx.Provider
      value={{
        isOpen,
        open: () => setOpen(true),
        close: () => setOpen(false),
        refreshKey,
        notifyCreated: () => setRefreshKey((k) => k + 1),
      }}
    >
      {children}
    </Ctx.Provider>
  )
}

export function useLeaveModal(): LeaveModalCtx {
  const ctx = useContext(Ctx)
  if (!ctx) throw new Error('useLeaveModal must be used within LeaveModalProvider')
  return ctx
}
