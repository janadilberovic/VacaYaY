'use client'

import { createContext, useContext, useCallback, useState, type ReactNode } from 'react'

type Kind = 'success' | 'error'
interface Toast {
  id: number
  msg: string
  kind: Kind
}

interface ToastCtx {
  toast: (msg: string, kind?: Kind) => void
}

const Ctx = createContext<ToastCtx | null>(null)
let nextId = 1

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([])

  const toast = useCallback((msg: string, kind: Kind = 'success') => {
    const id = nextId++
    setToasts((list) => [...list, { id, msg, kind }])
    setTimeout(() => setToasts((list) => list.filter((t) => t.id !== id)), 3800)
  }, [])

  return (
    <Ctx.Provider value={{ toast }}>
      {children}
      <div
        style={{
          position: 'fixed',
          right: 20,
          bottom: 20,
          display: 'flex',
          flexDirection: 'column',
          gap: 8,
          zIndex: 100,
        }}
      >
        {toasts.map((t) => (
          <div
            key={t.id}
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: 10,
              background: 'var(--surface)',
              border: '1px solid var(--border)',
              borderRadius: 11,
              padding: '12px 16px',
              boxShadow: 'var(--shadow)',
              fontSize: 13,
              fontWeight: 500,
              animation: 'pop .2s',
              maxWidth: 340,
            }}
          >
            <span
              style={{
                width: 8,
                height: 8,
                borderRadius: '50%',
                flexShrink: 0,
                background: t.kind === 'success' ? 'var(--pill-approved-fg)' : 'var(--pill-rejected-fg)',
              }}
            />
            {t.msg}
          </div>
        ))}
      </div>
    </Ctx.Provider>
  )
}

export function useToast(): ToastCtx {
  const ctx = useContext(Ctx)
  if (!ctx) throw new Error('useToast must be used within ToastProvider')
  return ctx
}
