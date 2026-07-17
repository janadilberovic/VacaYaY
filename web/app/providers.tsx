'use client'

import type { ReactNode } from 'react'
import { ThemeProvider } from '@/state/theme'
import { ToastProvider } from '@/state/toast'
import { AuthProvider } from '@/state/auth'

export function Providers({ children }: { children: ReactNode }) {
  return (
    <ThemeProvider>
      <ToastProvider>
        <AuthProvider>{children}</AuthProvider>
      </ToastProvider>
    </ThemeProvider>
  )
}
